// MapNode.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNode : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Image nodeImage;
    [SerializeField] private GameObject lockIcon;

    [Header("Sistema de Estrellas")]
    [SerializeField] private GameObject starsContainer; // El objeto padre "StarsEarned"
    [SerializeField] private Image[] starImages;        // Arrastra aquí Star1, Star2, Star3
    [SerializeField] private Sprite starOnSprite;       // La imagen brillante
    [SerializeField] private Sprite starOffSprite;      // La imagen apagada

    [Header("Colores")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color completedColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color bossColor = new Color(1f, 0.8f, 0.2f);

    private string _levelId;
    private string _language;
    private System.Action<string, string> _onClick;

    // AÑADIMOS "starsEarned" A LOS PARÁMETROS
    public void Setup(string id, string language, int index, bool unlocked, bool completed, int starsEarned, bool isBoss, System.Action<string, string> onClick)
    {
        _levelId = id;
        _language = language;
        _onClick = onClick;

        if (numberText != null) numberText.text = (index + 1).ToString();

        button.interactable = unlocked;

        // --- LÓGICA DE ESTADO Y COLOR ---
        if (!unlocked)
        {
            nodeImage.color = lockedColor;
            if (lockIcon != null) lockIcon.SetActive(true);
            if (starsContainer != null) starsContainer.SetActive(false); // Ocultar estrellas si está bloqueado
        }
        else
        {
            if (lockIcon != null) lockIcon.SetActive(false);
            if (starsContainer != null) starsContainer.SetActive(true); // Mostrar estrellas

            // Color del nodo
            if (isBoss) nodeImage.color = bossColor;
            else nodeImage.color = completed ? completedColor : unlockedColor;

            // --- LÓGICA DE LAS ESTRELLAS ---
            UpdateStars(starsEarned);
        }

        if (isBoss) transform.localScale = Vector3.one * 1.3f;
        else transform.localScale = Vector3.one;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(_language, _levelId));
    }

    // Función auxiliar para pintar estrellas
    private void UpdateStars(int amount)
    {
        if (starImages == null || starImages.Length == 0) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < amount)
            {
                // Si i es 0 y amount es 2 -> Pinta ON
                // Si i es 1 y amount es 2 -> Pinta ON
                starImages[i].sprite = starOnSprite;
            }
            else
            {
                // Si i es 2 y amount es 2 -> Pinta OFF
                starImages[i].sprite = starOffSprite;
            }
        }
    }
}