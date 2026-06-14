using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FailurePopupUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button explainButton;
    [SerializeField] private TextMeshProUGUI aiExplanationText;
    
    private string question;
    private string userAnswer;
    private string correctAnswer;

    private void Start()
    {
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (explainButton != null) explainButton.onClick.AddListener(OnExplain);
    }

    public void Setup(string q, string u, string c)
    {
        question = q;
        userAnswer = u;
        correctAnswer = c;

        if (aiExplanationText != null)
            aiExplanationText.text = ""; // Limpiar cualquier texto anterior

        if (explainButton != null)
            explainButton.interactable = true; // Rehabilitar el botón por si se usó antes

        gameObject.SetActive(true);
    }

    private void OnContinue()
    {
        gameObject.SetActive(false);
        // Avanzamos al siguiente minijuego de forma normal
        GameSceneManager manager = FindObjectOfType<GameSceneManager>();
        if (manager != null)
        {
            manager.NextMiniGame();
        }
    }

    private void OnExplain()
    {
        if (explainButton != null) explainButton.interactable = false;
        
        if (aiExplanationText != null)
            aiExplanationText.text = "Pensando respuesta...";

        // Formatear el prompt con instrucciones estrictas para respuestas cortas y naturales
        string prompt = $"Instrucciones: Responde en maximo 2 frases cortas. " +
                        $"No saludes. No uses emojis. No uses markdown ni asteriscos. No repitas la pregunta. " +
                        $"Ve directo al grano. Habla de tu a tu, como un companero de clase. " +
                        $"Explica solo por que la respuesta correcta lo es.\n\n" +
                        $"Pregunta: {question}\n" +
                        $"El alumno respondio: {userAnswer}\n" +
                        $"La respuesta correcta era: {correctAnswer}\n" +
                        $"Explicacion:";

        GeminiService gemini = FindObjectOfType<GeminiService>();
        if (gemini != null)
        {
            gemini.AskGemini(prompt, (response) => 
            {
                if (aiExplanationText != null)
                    aiExplanationText.text = response;
            });
        }
        else
        {
            if (aiExplanationText != null)
                aiExplanationText.text = "No se encontró el GeminiService en la escena.";
        }
    }
}
