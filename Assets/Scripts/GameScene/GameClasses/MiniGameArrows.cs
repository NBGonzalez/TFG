using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniGameArrows : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("UI")]
    public Transform leftColumn;
    public Transform rightColumn;
    public GameObject buttonPrefab; // ArrowButton

    private GameSceneManager manager;
    private MiniGameData data;

    private Button selectedLeft = null;
    private Button selectedRight = null;

    private Dictionary<Button, string> leftMap = new();
    private Dictionary<Button, string> rightMap = new();

    private int correctPairs = 0;

    public void Show(MiniGameData data, GameSceneManager mgr)
    {
        this.data = data;
        this.manager = mgr;

        if (titleText != null) titleText.text = data.title;
        if (instructionText != null) instructionText.text = data.content;


        ClearColumns();
        GenerateButtons();
    }

    private void ClearColumns()
    {
        foreach (Transform child in leftColumn) Destroy(child.gameObject);
        foreach (Transform child in rightColumn) Destroy(child.gameObject);
    }

    private void GenerateButtons()
    {
        List<PairData> pairs = data.pairs;

        // Clonar lista para desordenarla para la derecha
        List<PairData> shuffled = new List<PairData>(pairs);
        shuffled.Sort((a, b) => Random.Range(-1, 2));

        correctPairs = 0;

        // Crear botones LEFT
        foreach (var p in pairs)
        {
            GameObject go = Instantiate(buttonPrefab, leftColumn);
            var btn = go.GetComponent<Button>();
            go.GetComponentInChildren<TextMeshProUGUI>().text = p.left;

            leftMap[btn] = p.right;

            btn.onClick.AddListener(() => OnLeftSelected(btn));
        }

        // Crear botones RIGHT
        foreach (var p in shuffled)
        {
            GameObject go = Instantiate(buttonPrefab, rightColumn);
            var btn = go.GetComponent<Button>();
            go.GetComponentInChildren<TextMeshProUGUI>().text = p.right;

            rightMap[btn] = p.right;

            btn.onClick.AddListener(() => OnRightSelected(btn));
        }
    }

    // Cuando clicas botón LEFT
    private void OnLeftSelected(Button btn)
    {
        selectedLeft = btn;
        Highlight(btn, Color.yellow);
        TryCheckPair();
    }


    // Cuando clicas botón RIGHT
    private void OnRightSelected(Button btn)
    {
        selectedRight = btn;
        Highlight(btn, Color.yellow);
        TryCheckPair();
    }
    private void TryCheckPair()
    {
        if (selectedLeft == null || selectedRight == null) return;

        string expected = leftMap[selectedLeft];
        string selected = rightMap[selectedRight];

        if (expected == selected)
        {
            // Correct match
            Highlight(selectedLeft, Color.green);
            Highlight(selectedRight, Color.green);

            selectedLeft.interactable = false;
            selectedRight.interactable = false;

            correctPairs++;

            selectedLeft = null;
            selectedRight = null;

            if (correctPairs >= data.pairs.Count)
                StartCoroutine(NextMiniGame());
        }
        else
        {
            StartCoroutine(WrongPair(selectedLeft, selectedRight));
        }
    }


    private IEnumerator WrongPair(Button a, Button b)
    {
        Highlight(a, Color.red);
        Highlight(b, Color.red);

        yield return new WaitForSeconds(0.4f);

        Highlight(a, Color.white);
        Highlight(b, Color.white);

        selectedLeft = null;
        selectedRight = null;
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
        yield return new WaitForSeconds(0.8f);
        manager.NextMiniGame();
    }
}

