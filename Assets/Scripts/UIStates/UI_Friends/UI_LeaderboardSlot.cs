using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_LeaderboardSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI rankText;      // #1, #2...
    [SerializeField] private Image rankImage;               // (Opcional) Para medallas Oro/Plata
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI titleText;     // El texto del título (ej: "Novato")
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image backgroundPanel;         // Para cambiar color si soy yo

    [Header("Colores")]
    [SerializeField] private Color myColor = new Color(0.8f, 1f, 0.8f); // Verde clarito
    [SerializeField] private Color normalColor = Color.white;

    // Esta función la llamará el FriendState
    public void Setup(LeaderboardEntry data, ProfileAvatarSO avatarSO, ProfileTitleSO titleSO)
    {
        // 1. Textos Básicos
        rankText.text = $"#{data.rank}";
        nameText.text = data.userName;
        scoreText.text = data.score.ToString();

        // 2. Título (Usamos el dato del SO para saber nombre y color)
        if (titleSO != null)
        {
            titleText.text = titleSO.titleName;
            titleText.color = titleSO.titleColor;
        }
        else
        {
            titleText.text = "Desconocido";
            titleText.color = Color.gray;
        }

        // 3. Avatar
        if (avatarSO != null)
        {
            avatarImage.sprite = avatarSO.avatarImage;
        }

        // 4. ¿Soy yo? Resaltar fondo
        if (backgroundPanel != null)
        {
            backgroundPanel.color = data.isMe ? myColor : normalColor;
        }

        // EXTRA: Lógica simple para Top 3 (Opcional)
        // Podríamos cambiar el color del texto del Rango si es 1, 2 o 3
        if (data.rank == 1) rankText.color = Color.yellow;       // Oro
        else if (data.rank == 2) rankText.color = Color.gray;    // Plata
        else if (data.rank == 3) rankText.color = new Color(0.8f, 0.5f, 0.2f); // Bronce
        else rankText.color = Color.white;
    }
}