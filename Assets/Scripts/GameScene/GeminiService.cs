using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[System.Serializable]
public class GeminiRequest
{
    public List<Content> contents;
}

[System.Serializable]
public class Content
{
    public List<Part> parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

// Clases para deserializar la respuesta
[System.Serializable]
public class GeminiResponse
{
    public List<Candidate> candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

public class GeminiService : MonoBehaviour
{
    [Header("Configuración IA")]
    [Tooltip("Pega aquí tu API Key de Gemini")]
    public string apiKey;

    [Tooltip("Nombre del modelo. Opciones: gemini-1.5-flash-latest, gemini-1.5-pro, gemini-pro")]
    private string modelName = "gemini-2.5-flash";

    public void AskGemini(string prompt, System.Action<string> onResponse)
    {
        StartCoroutine(PostRequest(prompt, onResponse));
    }

    private IEnumerator PostRequest(string prompt, System.Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onResponse?.Invoke("Error: API Key de Gemini no configurada.");
            yield break;
        }

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
        Debug.Log("[GEMINI] Llamando a esta URL: " + url);
        GeminiRequest requestData = new GeminiRequest
        {
            contents = new List<Content>
            {
                new Content
                {
                    parts = new List<Part> { new Part { text = prompt } }
                }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // ¡AQUÍ ESTÁ EL CAMBIO! 
                // En vez de mostrar un texto genérico, forzamos a que el Pop-up 
                // te escriba EXACTAMENTE qué le pasa al móvil.
                string mensajeError = $"Error: {request.error}\nCódigo: {request.responseCode}";

                Debug.LogError(mensajeError);
                onResponse?.Invoke(mensajeError); // Mandamos el error real a la pantalla
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                // Extraer el texto de la respuesta (Parseo rápido)
                GeminiResponse responseObj = JsonUtility.FromJson<GeminiResponse>(jsonResponse);
                
                if (responseObj != null && responseObj.candidates != null && responseObj.candidates.Count > 0)
                {
                    string aiText = responseObj.candidates[0].content.parts[0].text;
                    onResponse?.Invoke(aiText);
                }
                else
                {
                    onResponse?.Invoke("Respuesta de la IA vacía o formato desconocido.");
                }
            }
        }
    }
}
