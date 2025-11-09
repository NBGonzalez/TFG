using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameQuizz : MonoBehaviour
{
    public static MiniGameQuizz Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<Button> optionButtons; // asigna los 4 botones
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    private GameSceneManager manager;
    private MiniGameData currentData;
    private bool answered = false;

    private void Awake() => Instance = this;

    public void Show(MiniGameData data, GameSceneManager mgr)
    {
        gameObject.SetActive(true);
        manager = mgr;
        currentData = data;
        answered = false;

        // Configurar textos
        titleText.text = data.title;
        questionText.text = data.question;

        // Configurar opciones
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < data.options.Count)
            {
                var btn = optionButtons[i];
                btn.gameObject.SetActive(true);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                text.text = data.options[i];

                // Resetear color
                var colors = btn.colors;
                colors.normalColor = normalColor;
                btn.colors = colors;

                btn.onClick.RemoveAllListeners();
                string optionText = data.options[i];
                btn.onClick.AddListener(() => OnOptionSelected(btn, optionText));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnOptionSelected(Button clickedButton, string selected)
    {
        if (answered) return; // evita clicks dobles
        answered = true;

        bool correct = selected == currentData.correctAnswer;
        var colors = clickedButton.colors;

        if (correct)
        {
            colors.normalColor = correctColor;
            clickedButton.colors = colors;

            // Esperar un poco y pasar al siguiente
            StartCoroutine(NextAfterDelay(1f));
        }
        else
        {
            colors.normalColor = wrongColor;
            clickedButton.colors = colors;
            answered = false; // permitir volver a intentar
        }
    }

    private System.Collections.IEnumerator NextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        manager.NextMiniGame();
    }
}
