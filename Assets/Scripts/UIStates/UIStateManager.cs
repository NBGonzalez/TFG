using System.Collections;
using UnityEngine;

public class UIStateManager : MonoBehaviour
{
    [SerializeField] private Transform uiRoot;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float slideDistance = 200f;

    private UIStateBase currentState;
    private bool isTransitioning = false;

    private void Start()
    {
        if (isTransitioning) return;
        ChangeState("Main");
    }

    public void ChangeState(string stateName)
    {
        
        StartCoroutine(ChangeStateRoutine(stateName));
        
    }

    private IEnumerator ChangeStateRoutine(string stateName)
    {
        isTransitioning = true;
        // Cargar el nuevo prefab
        GameObject prefab = Resources.Load<GameObject>($"UI/{stateName}UI");

        if (prefab == null)
        {
            Debug.LogError($"No se encontró el prefab en Resources/UI/{stateName}UI");
            yield break;
        }
        else
        {
            Debug.Log($"Prefab {prefab.name} encontrado correctamente");
        }


        // Instanciar el nuevo UI
        GameObject nextUI = Instantiate(prefab, uiRoot);
        var nextState = nextUI.GetComponent<UIStateBase>();
        nextState.Init(this);

        // Obtener CanvasGroups y RectTransforms
        CanvasGroup nextCG = nextUI.GetComponent<CanvasGroup>();
        RectTransform nextRT = nextUI.GetComponent<RectTransform>();

        if (currentState != null)
        {
            CanvasGroup currentCG = currentState.GetComponent<CanvasGroup>();
            RectTransform currentRT = currentState.GetComponent<RectTransform>();

            // Colocar el nuevo UI fuera de pantalla (a la derecha)
            nextRT.anchoredPosition = new Vector2(slideDistance, 0);
            nextCG.alpha = 0;

            // Animar transición
            float t = 0;
            while (t < transitionDuration)
            {
                t += Time.deltaTime;
                float lerp = t / transitionDuration;

                // Fade
                currentCG.alpha = Mathf.Lerp(1, 0, lerp);
                nextCG.alpha = Mathf.Lerp(0, 1, lerp);

                // Slide
                currentRT.anchoredPosition = new Vector2(Mathf.Lerp(0, -slideDistance, lerp), 0);
                nextRT.anchoredPosition = new Vector2(Mathf.Lerp(slideDistance, 0, lerp), 0);

                yield return null;
            }

            // Terminar transición
            currentCG.alpha = 0;
            nextCG.alpha = 1;
            currentRT.anchoredPosition = new Vector2(-slideDistance, 0);
            nextRT.anchoredPosition = Vector2.zero;

            // Destruir la pantalla anterior
            currentState.OnExit();
            Destroy(currentState.gameObject);
        }

        currentState = nextState;
        nextState.OnEnter();
        isTransitioning = false;
    }
}