// MiniGameArrows.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MiniGameArrows : MonoBehaviour, IMiniGame
{
    [Header("UI")]
    [SerializeField] private Transform leftColumn;   // container (VerticalLayoutGroup)
    [SerializeField] private Transform rightColumn;  // container (VerticalLayoutGroup)
    [SerializeField] private GameObject buttonPrefab; // prefab botón (con TMP hijo)

    private MiniGameData data;
    private MiniGameBaseClass baseUI;

    private Button selectedLeft = null;
    private Button selectedRight = null;

    private Dictionary<Button, string> leftMap = new();
    private Dictionary<Button, string> rightMap = new();

    private List<Button> generatedButtons = new(); // para TearDown
    private int correctPairs = 0;

    public void Initialize(MiniGameData data, MiniGameBaseClass baseUI)
    {
        this.data = data;
        this.baseUI = baseUI;
        correctPairs = 0;

        ClearColumns();
        GenerateButtons();
        // El texto principal ya lo pone baseUI (data.content).
    }

    public void TearDown()
    {
        // Limpiar listeners y destruir botones
        foreach (var btn in generatedButtons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }
        }
        generatedButtons.Clear();
        leftMap.Clear();
        rightMap.Clear();
        selectedLeft = null;
        selectedRight = null;
    }

    private void ClearColumns()
    {
        if (leftColumn != null)
        {
            foreach (Transform t in leftColumn) Destroy(t.gameObject);
        }
        if (rightColumn != null)
        {
            foreach (Transform t in rightColumn) Destroy(t.gameObject);
        }

        leftMap.Clear();
        rightMap.Clear();
        generatedButtons.Clear();
    }

    private void GenerateButtons()
    {
        if (data?.pairs == null || data.pairs.Count == 0) return;

        // Orden original para la izquierda y desordenado para la derecha
        List<PairData> leftList = data.pairs;
        List<PairData> rightList = new List<PairData>(data.pairs);
        rightList = rightList.OrderBy(x => Random.value).ToList();

        // LEFT
        foreach (var p in leftList)
        {
            GameObject go = Instantiate(buttonPrefab, leftColumn);
            Button btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = p.left;

            leftMap[btn] = p.right;
            generatedButtons.Add(btn);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnLeftClicked(btn));
            baseUI.SetButtonColor(btn, Color.white);
        }

        // RIGHT
        foreach (var p in rightList)
        {
            GameObject go = Instantiate(buttonPrefab, rightColumn);
            Button btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = p.right;

            rightMap[btn] = p.right;
            generatedButtons.Add(btn);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnRightClicked(btn));
            baseUI.SetButtonColor(btn, Color.white);
        }
    }

    private void OnLeftClicked(Button btn)
    {
        if (!btn.interactable) return;

        if (selectedLeft == btn)
        {
            baseUI.SetButtonColor(btn, Color.white);
            selectedLeft = null;
        }
        else
        {
            if (selectedLeft != null) baseUI.SetButtonColor(selectedLeft, Color.white);
            selectedLeft = btn;
            baseUI.SetButtonColor(btn, Color.yellow);
        }

        TryCheckPair();
    }

    private void OnRightClicked(Button btn)
    {
        if (!btn.interactable) return;

        if (selectedRight == btn)
        {
            baseUI.SetButtonColor(btn, Color.white);
            selectedRight = null;
        }
        else
        {
            if (selectedRight != null) baseUI.SetButtonColor(selectedRight, Color.white);
            selectedRight = btn;
            baseUI.SetButtonColor(btn, Color.yellow);
        }

        TryCheckPair();
    }

    private void TryCheckPair()
    {
        if (selectedLeft == null || selectedRight == null) return;

        string expected = leftMap.ContainsKey(selectedLeft) ? leftMap[selectedLeft] : null;
        string chosen = rightMap.ContainsKey(selectedRight) ? rightMap[selectedRight] : null;

        bool ok = expected != null && chosen != null && expected == chosen;

        if (ok)
        {
            // Reporta a la clase base un acierto, y esta a su vez al manager
            baseUI.ReportSuccess();

            baseUI.SetButtonColor(selectedLeft, Color.green);
            baseUI.SetButtonColor(selectedRight, Color.green);

            selectedLeft.interactable = false;
            selectedRight.interactable = false;

            correctPairs++;
            selectedLeft = null;
            selectedRight = null;

            if (correctPairs >= data.pairs.Count)
            {
                StartCoroutine(baseUI.NextMiniGameDelayed(0.7f));
            }
        }
        else
        {
            // Reporta a la clase base un fallo, y esta a su vez al manager
            baseUI.ReportFailure();

            StartCoroutine(HandleWrongPair(selectedLeft, selectedRight));
        }
    }

    private IEnumerator HandleWrongPair(Button a, Button b)
    {
        if (a != null) baseUI.SetButtonColor(a, Color.red);
        if (b != null) baseUI.SetButtonColor(b, Color.red);

        yield return new WaitForSeconds(0.4f);

        if (a != null) baseUI.SetButtonColor(a, Color.white);
        if (b != null) baseUI.SetButtonColor(b, Color.white);

        selectedLeft = null;
        selectedRight = null;
    }
}
