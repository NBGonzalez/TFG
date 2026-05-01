using UnityEngine;
using System.IO;
using System;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }

    // Evento que avisa a la UI: enviamos Título y Autor
    public static event Action<string, string> OnItineraryImported;

    private string _lastProcessedIntentData = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Esperamos un segundo a que todo cargue
        Invoke(nameof(CheckForIncomingFile), 1.5f);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus) Invoke(nameof(CheckForIncomingFile), 0.5f);
    }

    private void CheckForIncomingFile()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass  unityPlayer     = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent          = currentActivity.Call<AndroidJavaObject>("getIntent");
            string            action          = intent.Call<string>("getAction");

            if (action == "android.intent.action.VIEW")
            {
                AndroidJavaObject uri = intent.Call<AndroidJavaObject>("getData");
                if (uri != null)
                {
                    string uriString = uri.Call<string>("toString");
                    if (uriString == _lastProcessedIntentData) return;
                    _lastProcessedIntentData = uriString;

                    // Si es un archivo que nos interesa, lo importamos
                    if (uriString.Contains(".itinerario") || uriString.Contains("octet-stream") || uriString.StartsWith("content://"))
                    {
                        ImportItinerary(uri);
                    }
                }
            }
        }
        catch (Exception e) { Debug.LogError("[DeepLink] Error: " + e.Message); }
#endif
    }

    private void ImportItinerary(AndroidJavaObject uri)
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");

            AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri);
            AndroidJavaObject isr = new AndroidJavaObject("java.io.InputStreamReader", inputStream);
            AndroidJavaObject bufferedReader = new AndroidJavaObject("java.io.BufferedReader", isr);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string line;
            while ((line = bufferedReader.Call<string>("readLine")) != null)
            {
                sb.AppendLine(line);
            }
            bufferedReader.Call("close");

            string jsonRecibido = sb.ToString();
            if (string.IsNullOrWhiteSpace(jsonRecibido)) return;

            CustomItineraryData data = JsonUtility.FromJson<CustomItineraryData>(jsonRecibido);
            if (data == null || string.IsNullOrEmpty(data.itineraryId)) return;

            // Guardamos físicamente el archivo
            string idLimpio = data.itineraryId.StartsWith("custom-") ? data.itineraryId : "custom-" + data.itineraryId;
            string rutaDestino = Path.Combine(Application.persistentDataPath, idLimpio + ".json");
            File.WriteAllText(rutaDestino, jsonRecibido);

            // Avisamos a la UI para que haga Refresh y muestre el Popup
            OnItineraryImported?.Invoke(data.title, data.authorName);
        }
        catch (Exception e) { Debug.LogError("[DeepLink] Fallo importación: " + e.Message); }
    }
}