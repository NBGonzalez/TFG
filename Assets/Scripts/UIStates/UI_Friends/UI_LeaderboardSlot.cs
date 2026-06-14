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
    [SerializeField] private TextMeshProUGUI titleText;     // El texto del titulo (ej: "Novato")
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Destacar jugador")]
    [SerializeField] private Image myHighlightImage;        // Imagen con shader para resaltar cuando soy yo

    // Esta funcion la llamara el FriendState
    public void Setup(LeaderboardEntry data, ProfileAvatarSO avatarSO, ProfileTitleSO titleSO)
    {
        // 1. Textos Basicos
        rankText.text = $"#{data.rank}";
        nameText.text = data.userName;
        scoreText.text = data.score.ToString();

        // 2. Titulo (Usamos el dato del SO para saber nombre y color)
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

        // 4. Soy yo? Activar la imagen de resaltado con el CorrectColor
        if (myHighlightImage != null)
        {
            if (data.isMe && AppColorManager.Instance != null)
            {
                myHighlightImage.gameObject.SetActive(true);
                Color highlightColor = AppColorManager.Instance.CorrectColor;
                highlightColor.a = 1f;
                myHighlightImage.color = highlightColor;
            }
            else
            {
                myHighlightImage.gameObject.SetActive(false);
            }
        }

        // EXTRA: Logica simple para Top 3 (Opcional)
        if (data.rank == 1) rankText.color = Color.yellow;       // Oro
        else if (data.rank == 2) rankText.color = Color.gray;    // Plata
        else if (data.rank == 3) rankText.color = new Color(0.8f, 0.5f, 0.2f); // Bronce
        else rankText.color = Color.white;
    }
}