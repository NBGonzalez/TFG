// HelpButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boton de ayuda reutilizable. Va en un prefab autocontenido que incluye el
/// boton "?", el panel explicativo y el boton "Ok". Se arrastra a cualquier escena
/// y solo hay que cambiar el campo helpText en el Inspector (es un override por instancia).
/// </summary>
public class HelpButton : MonoBehaviour
{
    [Header("Texto de ayuda (cambialo en cada instancia desde el Inspector)")]
    [TextArea(3, 10)]
    [SerializeField] private string helpText;

    [Header("Referencias del prefab")]
    [Tooltip("El boton '?' que abre el panel")]
    [SerializeField] private Button helpButton;

    [Tooltip("El panel que se muestra/oculta (debe estar INACTIVO por defecto en el prefab)")]
    [SerializeField] private GameObject helpPanel;

    [Tooltip("El texto (TMP) dentro del panel donde se vuelca helpText")]
    [SerializeField] private TMP_Text helpTextLabel;

    [Tooltip("El boton 'Ok' que cierra el panel")]
    [SerializeField] private Button okButton;
    
    [Tooltip("El botón de fondo para que el usuario pueda cerrar el panel al pulsar fuera.")]
    [SerializeField] private Button backGroundButton;

    private void Awake()
    {
        // Aseguramos que el panel arranca cerrado, pase lo que pase en el prefab.
        if (helpPanel != null) helpPanel.SetActive(false);

        if (helpButton != null) helpButton.onClick.AddListener(OpenPanel);
        if (okButton != null) okButton.onClick.AddListener(ClosePanel);
        if (backGroundButton != null) backGroundButton.onClick.AddListener(ClosePanel);
    }

    private void OnDestroy()
    {
        // Limpiamos los listeners para no dejar referencias colgando.
        if (helpButton != null) helpButton.onClick.RemoveListener(OpenPanel);
        if (okButton != null) okButton.onClick.RemoveListener(ClosePanel);
    }

    /// <summary>Abre el panel y vuelca el texto configurado en el Inspector.</summary>
    public void OpenPanel()
    {
        if (helpTextLabel != null) helpTextLabel.text = helpText;
        if (helpPanel != null) helpPanel.SetActive(true);
    }

    /// <summary>Cierra el panel.</summary>
    public void ClosePanel()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
    }
}