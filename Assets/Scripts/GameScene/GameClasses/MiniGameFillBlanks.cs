using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameFillBlanks : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("UI")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject buttonPrefab;

    private GameSceneManager manager;
    private MiniGameData data;

    public void Show(MiniGameData data, GameSceneManager mgr)
    {
        this.data = data;
        this.manager = mgr;

        // Set UI
        if (titleText != null) titleText.text = data.title;
        if (questionText != null) questionText.text = data.question;

        ClearOptions();
        GenerateButtons();
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
    }

    private void GenerateButtons()
    {
        // Copia de lista y mezclar
        List<string> shuffled = new List<string>(data.options);
        shuffled.Sort((a, b) => Random.Range(-1, 2));

        foreach (var option in shuffled)
        {
            GameObject go = Instantiate(buttonPrefab, optionsContainer);
            Button btn = go.GetComponent<Button>();
            TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

            txt.text = option;

            btn.onClick.AddListener(() => OnOptionSelected(btn, option));
        }
    }

    private void OnOptionSelected(Button btn, string selectedOption)
    {
        bool correct = selectedOption == data.correctAnswer;

        if (correct)
        {
            Highlight(btn, Color.green);
            StartCoroutine(NextMiniGame());
        }
        else
        {
            StartCoroutine(WrongAnswer(btn));
        }
    }

    private IEnumerator WrongAnswer(Button btn)
    {
        Highlight(btn, Color.red);
        yield return new WaitForSeconds(0.4f);
        Highlight(btn, Color.white);
    }

    private void Highlight(Button btn, Color c)
    {
        var colors = btn.colors;
        colors.normalColor = c;
        colors.highlightedColor = c;
        colors.pressedColor = c;
        colors.selectedColor = c;
        btn.colors = colors;
    }

    private IEnumerator NextMiniGame()
    {
        yield return new WaitForSeconds(0.7f);
        manager.NextMiniGame();
    }
}
