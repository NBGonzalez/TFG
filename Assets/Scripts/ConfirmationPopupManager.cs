//ConfirmationPopupManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // 🌟 OBLIGATORIO para usar 'Action'

public class ConfirmationPopupManager : MonoBehaviour
{
    public static ConfirmationPopupManager Instance { get; private set; }

    [Header("Componentes UI")]
    [SerializeField] private GameObject popupContainer; // El panel que se enciende/apaga
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // LA VARIABLE MÁGICA: Aquí guardaremos temporalmente "lo que hay que hacer"
    private Action actionToExecuteIfYes;

    private void Awake()
    {
        // Configuración Singleton básica para poder llamarlo desde cualquier escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: si quieres que sobreviva entre escenas
            popupContainer.SetActive(false); // Empieza oculto
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Configurar los listeners fijos una sola vez
        yesButton.onClick.AddListener(OnYesPressed);
        noButton.onClick.AddListener(OnNoPressed);
    }

    // ========================================================
    // EL METODO MAESTRO (La API pública que usará toda tu app)
    // ========================================================
    public void RequestConfirmation(string message, Action confirmedAction)
    {
        messageText.text = message;
        actionToExecuteIfYes = confirmedAction; // Guardamos la función en la recámara

        popupContainer.SetActive(true); // Mostramos el Pop-up
    }

    private void OnYesPressed()
    {
        popupContainer.SetActive(false); // Cerramos el Pop-up

        // Ejecutamos la función que teníamos guardada
        actionToExecuteIfYes?.Invoke();

        actionToExecuteIfYes = null; // Limpiamos por seguridad
    }

    private void OnNoPressed()
    {
        popupContainer.SetActive(false); // Simplemente cerramos
        actionToExecuteIfYes = null; // Tiramos la función a la basura, no se ejecuta
    }
}