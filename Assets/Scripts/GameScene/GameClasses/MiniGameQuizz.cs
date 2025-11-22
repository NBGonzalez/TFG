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
        if (answered) return;

        bool correct = selected == currentData.correctAnswer;

        var colors = clickedButton.colors;

        if (correct)
        {
            SetButtonColor(clickedButton, correctColor);
            answered = true;
            StartCoroutine(NextAfterDelay(0.6f));
        }
        else
        {
            answered = false;

            StartCoroutine(FlashWrong(clickedButton));
        }
    }
    private void SetButtonColor(Button btn, Color c)
    {
        var colors = btn.colors;
        colors.normalColor = c;
        colors.highlightedColor = c;
        colors.pressedColor = c;
        colors.selectedColor = c;
        btn.colors = colors;
    }
    private System.Collections.IEnumerator FlashWrong(Button btn)
    {
        SetButtonColor(btn, wrongColor);
        yield return new WaitForSeconds(0.25f);
        SetButtonColor(btn, normalColor);
    }


    private System.Collections.IEnumerator NextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        manager.NextMiniGame();
    }
}
