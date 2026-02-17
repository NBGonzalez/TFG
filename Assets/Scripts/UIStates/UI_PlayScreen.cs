//UI_PlayScreen.cs
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_PlayScreen : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown languageDropdown;
    public RectTransform contentRoot; // El Content del ScrollView

    [Header("NUEVOS PREFABS")]
    public GameObject mapNodePrefab;
    public GameObject pathLinePrefab;

    [Header("Configuración Mapa")]
    public float verticalSpacing = 200f;
    public float xAmplitude = 200f;
    public float waveFrequency = 1f;

    [Header("Referencias UI")]
    public LevelInfoPopup infoPopup;

    private PathModel currentPath;

    private void Start()
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "SQL", "C++", "Python" });
        languageDropdown.onValueChanged.AddListener((val) => LoadLanguage(languageDropdown.options[val].text));
        LoadLanguage("SQL");
    }

    private void LoadLanguage(string language)
    {
        StartCoroutine(LoadLanguageAsync(language));
    }

    private IEnumerator LoadLanguageAsync(string language)
    {
        string fileName = "";
        switch (language)
        {
            case "SQL": fileName = "sql_path.json"; break;
            case "C++": fileName = "cpp_path.json"; break;
            case "Python": fileName = "python_path.json"; break;
        }

        string path = Path.Combine(Application.streamingAssetsPath, "paths", fileName);
        string json = "";

        if (path.Contains("://") || path.Contains(":///"))
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success) { Debug.LogError("Error: " + www.error); yield break; }
                json = www.downloadHandler.text;
            }
        }
        else
        {
            if (!File.Exists(path)) { Debug.LogError("No file: " + path); yield break; }
            json = File.ReadAllText(path);
        }

        currentPath = JsonUtility.FromJson<PathModel>(json);
        GenerateMap(language);
    }

    private void GenerateMap(string language)
    {
        // 1. Limpiar hijos
        foreach (Transform child in contentRoot) Destroy(child.gameObject);

        // --- RESET TOTAL ---
        contentRoot.anchorMin = new Vector2(0.5f, 1f);
        contentRoot.anchorMax = new Vector2(0.5f, 1f);
        contentRoot.pivot = new Vector2(0.5f, 1f);
        contentRoot.anchoredPosition = Vector2.zero;

        var progress = PlayerProgressManager.Instance;
        int count = currentPath.levels.Length;

        // 2. Calcular Altura
        float paddingBottom = 1000f;
        float totalHeight = (count * verticalSpacing) + paddingBottom;
        contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, totalHeight);

        // --- VARIABLES DE POSICIÓN ---
        // Usaremos 'prevCenterPos' para guardar el CENTRO del nodo anterior, no su parte de arriba.
        Vector2 prevCenterPos = Vector2.zero;
        bool isFirst = true;
        float lastUnlockedY = 0f;

        // 3. Generar Nodos
        for (int i = 0; i < count; i++)
        {
            var lvl = currentPath.levels[i];

            bool unlocked = (i == 0);
            if (i > 0)
            {
                var prevLevel = currentPath.levels[i - 1];
                if (progress.IsLevelCompleted(language, prevLevel.id)) unlocked = true;
            }
            bool completed = progress.IsLevelCompleted(language, lvl.id);

            // Posición ANCLAJE (Parte superior del nodo)
            float posY = -150f - (i * verticalSpacing);
            float posX = Mathf.Sin(i * waveFrequency) * xAmplitude;
            Vector2 currentAnchorPos = new Vector2(posX, posY);

            if (unlocked) lastUnlockedY = posY;

            // Instanciar
            GameObject nodeGO = Instantiate(mapNodePrefab, contentRoot);
            RectTransform nodeRect = nodeGO.GetComponent<RectTransform>();

            // Forzar anclajes
            nodeRect.anchorMin = new Vector2(0.5f, 1f);
            nodeRect.anchorMax = new Vector2(0.5f, 1f);
            nodeRect.pivot = new Vector2(0.5f, 1f);

            nodeRect.anchoredPosition = currentAnchorPos;

            MapNode nodeScript = nodeGO.GetComponent<MapNode>();
            bool isBoss = (i + 1) % 5 == 0;
            int stars = progress.GetStarsForLevel(language, lvl.id);

            // Configuramos el nodo (Esto aplicará la escala si es Boss)
            nodeScript.Setup(lvl.id, language, i, unlocked, completed, stars, isBoss,
                (lang, id) => OpenPopup(lvl, lang, stars));

            // --- CÁLCULO DEL CENTRO VISUAL (MAGIA AQUÍ) ---
            // 1. Obtenemos la altura del nodo (normalmente 100px o lo que mida tu prefab)
            float nodeHeight = nodeRect.rect.height;

            // 2. Obtenemos la escala (si es Boss, será 1.3, si no 1.0)
            float scale = nodeGO.transform.localScale.y;

            // 3. Calculamos cuánto hay que bajar para llegar al centro
            // (Altura / 2) * Escala
            float offsetToCenter = (nodeHeight / 2f) * scale;

            // 4. Posición del Centro = Posición Arriba - Offset
            Vector2 currentCenterPos = currentAnchorPos - new Vector2(0, offsetToCenter);
            // ----------------------------------------------

            // Dibujamos la línea de CENTRO a CENTRO
            if (!isFirst) CreateConnection(prevCenterPos, currentCenterPos);

            // Guardamos el centro actual para la siguiente vuelta
            prevCenterPos = currentCenterPos;
            isFirst = false;
        }

        // 4. Iniciar AutoScroll
        StartCoroutine(ApplyAutoScroll(lastUnlockedY, totalHeight));
    }

    private IEnumerator ApplyAutoScroll(float targetY, float contentTotalHeight)
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        ScrollRect scrollRect = contentRoot.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) scrollRect.velocity = Vector2.zero;

        float viewportHeight = 0f;
        if (contentRoot.parent != null)
        {
            RectTransform viewport = contentRoot.parent as RectTransform;
            viewportHeight = viewport.rect.height;
        }
        if (viewportHeight <= 0) viewportHeight = Screen.height;

        Debug.Log($"[AutoScroll] Viewport Height: {viewportHeight}");

        float targetY_Positive = Mathf.Abs(targetY);
        float centerOffset = viewportHeight * 0.5f;
        float finalPos = targetY_Positive - centerOffset;

        float maxScrollPossible = contentTotalHeight - viewportHeight;
        if (maxScrollPossible < 0) maxScrollPossible = 0;

        finalPos = Mathf.Clamp(finalPos, 0, maxScrollPossible);

        contentRoot.anchoredPosition = new Vector2(contentRoot.anchoredPosition.x, finalPos);
    }

    private void CreateConnection(Vector2 posA, Vector2 posB)
    {
        GameObject lineGO = Instantiate(pathLinePrefab, contentRoot);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();

        // --- SEGURIDAD PARA LÍNEAS ---
        // ¡IMPORTANTE! Las líneas también necesitan saber que el (0,0) es el techo.
        lineRect.anchorMin = new Vector2(0.5f, 1f);
        lineRect.anchorMax = new Vector2(0.5f, 1f);
        lineRect.pivot = new Vector2(0.5f, 0.5f); // El pivote de la línea SÍ va al centro para rotar bien
        lineGO.transform.SetAsFirstSibling();

        Vector2 midPoint = (posA + posB) / 2f;
        lineRect.anchoredPosition = midPoint;

        Vector2 dir = posB - posA;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        float dist = Vector2.Distance(posA, posB);
        lineRect.sizeDelta = new Vector2(dist, 20f);
        //lineRect.transform.position.y -= 10f; // Ajuste vertical para que la línea quede centrada entre los nodos
    }

    private void OpenPopup(LevelModel levelData, string language, int stars)
    {
        infoPopup.Show(levelData, language, stars);
    }
}