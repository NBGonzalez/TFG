using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundTransition : MonoBehaviour
{
    public static BackgroundTransition Instance;

    [Header("References")]
    public Image backgroundImage;

    private Material backgroundMaterial;
    private Coroutine transicionActiva;

    private float currentShaderVal = 0f;
    private float currentUIAlpha = 1f;

    private Canvas miCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            miCanvas = GetComponent<Canvas>();

            backgroundMaterial = new Material(backgroundImage.material);
            backgroundImage.material = backgroundMaterial;
            backgroundMaterial.SetFloat("_ProgresoTransicion", currentShaderVal);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // EL CÓDIGO APISONADORA (A prueba de balas)
    // ==========================================
    void Update()
    {
        if (miCanvas != null)
        {
            // 1. Forzamos los valores físicos para que el fondo NUNCA tape a la victoria
            miCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            miCanvas.planeDistance = 150f; // Lo mandamos muy al fondo (tu victoria estará a 100)
            miCanvas.sortingOrder = -10;

            // 2. Si el Canvas está ciego... buscamos cámara por la fuerza bruta
            if (miCanvas.worldCamera == null)
            {
                // Resources.FindObjectsOfTypeAll busca TODO, incluso cámaras apagadas o en prefabricados
                Camera[] todasLasCamaras = Resources.FindObjectsOfTypeAll<Camera>();

                foreach (Camera cam in todasLasCamaras)
                {
                    // Filtramos para asegurarnos de que es una cámara real de la escena
                    if (cam.gameObject.scene.isLoaded && cam.gameObject.hideFlags == HideFlags.None)
                    {
                        miCanvas.worldCamera = cam;
                        Debug.Log("🎯 ¡Cámara atrapada por la fuerza bruta!: " + cam.name);
                        break;
                    }
                }
            }
        }
    }

    public void ToggleTransitionAndLoad(string nextSceneName)
    {
        if (transicionActiva != null)
        {
            StopCoroutine(transicionActiva);
        }

        transicionActiva = StartCoroutine(AnimateAndLoad(nextSceneName));
    }

    private IEnumerator AnimateAndLoad(string targetScene)
    {
        float shaderSpeed = 1f / 1.5f;
        float uiSpeed = 1f / 0.25f;

        bool faseDeOcultacion = !string.IsNullOrEmpty(targetScene);
        float targetShader = faseDeOcultacion ? 1f : 0f;
        float targetUI = faseDeOcultacion ? 0f : 1f;

        while (currentShaderVal != targetShader || currentUIAlpha != targetUI)
        {
            currentShaderVal = Mathf.MoveTowards(currentShaderVal, targetShader, shaderSpeed * Time.deltaTime);
            currentUIAlpha = Mathf.MoveTowards(currentUIAlpha, targetUI, uiSpeed * Time.deltaTime);

            backgroundMaterial.SetFloat("_ProgresoTransicion", currentShaderVal);

            List<CanvasGroup> interfacesUI = BuscarTodosLosCanvasUI();
            foreach (var ui in interfacesUI)
            {
                ui.alpha = currentUIAlpha;
            }

            yield return null;
        }

        if (faseDeOcultacion)
        {
            AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(targetScene);

            while (!operacionCarga.isDone)
            {
                yield return null;
            }

            // Destruimos la referencia a la cámara vieja. 
            // Esto despertará a nuestro Update() para que atrape la nueva.
            if (miCanvas != null)
            {
                miCanvas.worldCamera = null;
            }

            float tiempoEspera = 0f;
            while (tiempoEspera < 0.1f)
            {
                tiempoEspera += Time.deltaTime;

                List<CanvasGroup> canvasRecienNacidos = BuscarTodosLosCanvasUI();
                foreach (var ui in canvasRecienNacidos)
                {
                    ui.alpha = 0f;
                    currentUIAlpha = 0f;
                }

                yield return null;
            }

            transicionActiva = StartCoroutine(AnimateAndLoad(""));
        }
    }

    private List<CanvasGroup> BuscarTodosLosCanvasUI()
    {
        List<CanvasGroup> gruposUI = new List<CanvasGroup>();
        Canvas[] todosLosCanvas = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in todosLosCanvas)
        {
            // Solo cogemos los Canvas principales y que NO sean el del fondo
            if (canvas.transform.parent == null && canvas.GetComponent<BackgroundTransition>() == null)
            {
                CanvasGroup cg = canvas.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = canvas.gameObject.AddComponent<CanvasGroup>();
                }
                gruposUI.Add(cg);
            }
        }
        return gruposUI;
    }
}