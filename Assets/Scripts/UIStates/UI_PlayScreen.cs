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
    public GameObject mapNodePrefab;  // Arrastra aquí el nuevo prefab Redondo
    public GameObject pathLinePrefab; // Arrastra aquí el nuevo prefab Línea

    [Header("Configuración Mapa")]
    public float verticalSpacing = 200f; // Distancia entre niveles
    public float xAmplitude = 200f;      // Anchura de la curva
    public float waveFrequency = 1f;     // Frecuencia de la curva

    [Header("Referencias UI")]
    public LevelInfoPopup infoPopup; // <--- ARRASTRA AQUÍ TU NUEVO POPUP

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
        // --- ESTO ES TU CÓDIGO ORIGINAL (INTACTO) ---
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

        // Llamamos a la nueva función de mapa
        GenerateMap(language);
    }

    // --- NUEVA FUNCIÓN: GENERAR MAPA EN ZIG-ZAG ---
    private void GenerateMap(string language)
    {
        // 1. Limpiar lo viejo
        foreach (Transform child in contentRoot) Destroy(child.gameObject);

        var progress = PlayerProgressManager.Instance;
        int count = currentPath.levels.Count();

        // 2. Ajustar altura del contenedor (IMPORTANTE)
        float totalHeight = (count * verticalSpacing) + 400f;
        contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, totalHeight);

        Vector2 prevPos = Vector2.zero;
        bool isFirst = true;

        // --- VARIABLE PARA EL AUTO-SCROLL ---
        float lastUnlockedY = 0f; // Aquí guardaremos la posición Y del último nivel disponible

        for (int i = 0; i < count; i++)
        {
            var lvl = currentPath.levels[i];

            // Comprobamos estado
            bool unlocked = false;

            if (i == 0)
            {
                // El primero siempre está abierto
                unlocked = true;
            }
            else
            {
                // A partir del segundo, miramos si el ANTERIOR (i-1) está completado
                var prevLevel = currentPath.levels[i - 1];
                if (progress.IsLevelCompleted(language, prevLevel.id))
                {
                    unlocked = true;
                }
            }
            bool completed = progress.IsLevelCompleted(language, lvl.id);

            // --- CÁLCULO DE POSICIÓN (Zig-Zag) ---
            float posY = -150f - (i * verticalSpacing); // Hacia abajo
            float posX = Mathf.Sin(i * waveFrequency) * xAmplitude; // Onda
            Vector2 currentPos = new Vector2(posX, posY);

            // --- ACTUALIZAR PUNTO DE SCROLL ---
            // Si este nivel está desbloqueado, actualizamos la "meta" del scroll
            if (unlocked)
            {
                lastUnlockedY = posY;
            }

            // --- INSTANCIAR NODO ---
            GameObject nodeGO = Instantiate(mapNodePrefab, contentRoot);
            RectTransform nodeRect = nodeGO.GetComponent<RectTransform>();
            nodeRect.anchoredPosition = currentPos;

            // Configurar Script
            MapNode nodeScript = nodeGO.GetComponent<MapNode>();
            bool isBoss = (i + 1) % 5 == 0; // Cada 5 niveles es Boss

            // Obtener estrellas guardadas (Si no tienes este método, pon 0 de momento o invéntalo)
            int stars = progress.GetStarsForLevel(language, lvl.id);

            nodeScript.Setup(lvl.id, language, i, unlocked, completed, stars, isBoss,
                (lang, id) => OpenPopup(lvl, lang, stars));

            // --- DIBUJAR LÍNEA ---
            if (!isFirst)
            {
                CreateConnection(prevPos, currentPos);
            }

            prevPos = currentPos;
            isFirst = false;
        }
        StartCoroutine(ApplyAutoScroll(lastUnlockedY, totalHeight));
    }
    // Cambiamos 'void' por 'IEnumerator'
    private IEnumerator ApplyAutoScroll(float targetY, float contentTotalHeight)
    {
        // --- LA MAGIA: ESPERAR UN FRAME ---
        // Esto deja que Unity actualice la UI, los tamaños y el ScrollRect
        yield return null;

        // Truco extra: Forzar la actualización de los Canvas por si acaso
        Canvas.ForceUpdateCanvases();

        // 1. Obtener la altura de la ventana visible (Viewport)
        float viewportHeight = 0f;
        if (contentRoot.parent != null)
        {
            RectTransform viewport = contentRoot.parent as RectTransform;
            if (viewport != null) viewportHeight = viewport.rect.height;
        }

        if (viewportHeight == 0) viewportHeight = Screen.height;

        // 2. Calcular la posición ideal
        // targetY es negativo. Lo pasamos a positivo.
        // Restamos mitad de pantalla para centrar.
        float finalContentY = Mathf.Abs(targetY) - (viewportHeight * 0.5f);

        // 3. Limitar (Clamp)
        // OJO: contentTotalHeight debe ser mayor que viewportHeight para hacer scroll
        float maxScroll = Mathf.Max(0, contentTotalHeight - viewportHeight);

        finalContentY = Mathf.Clamp(finalContentY, 0, maxScroll);

        // 4. Aplicar
        contentRoot.anchoredPosition = new Vector2(contentRoot.anchoredPosition.x, finalContentY);

        Debug.Log($"[AutoScroll] TargetY: {targetY} | FinalPos: {finalContentY}");
    }
    private void OpenPopup(LevelModel levelData, string language, int stars)
    {
        Debug.Log($"Abriendo info de: {levelData.title}");

        // Le pasamos todo al Popup
        infoPopup.Show(levelData, language, stars);
    }

    private void CreateConnection(Vector2 posA, Vector2 posB)
    {
        GameObject lineGO = Instantiate(pathLinePrefab, contentRoot);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();

        // Poner la línea al fondo
        lineGO.transform.SetAsFirstSibling();

        // Matemáticas para colocar y rotar la línea
        Vector2 midPoint = (posA + posB) / 2f;
        lineRect.anchoredPosition = midPoint;

        Vector2 dir = posB - posA;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        float dist = Vector2.Distance(posA, posB);
        lineRect.sizeDelta = new Vector2(dist, 20f); // 20f es el grosor
    }

    //private void OnLevelClicked(string language, string levelId)
    //{
    //    Debug.Log($"Abriendo nivel {levelId}");
    //    GameManager.Instance.SetCurrentLevel(language, levelId);
    //    SceneManager.LoadScene("GameScene");
    //}
}