using System.Collections;
using UnityEngine;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class UIStateManager : MonoBehaviour
{
    [SerializeField] private Transform uiRoot;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float slideDistance = 200f;

    private UIStateBase currentState;
    private bool isTransitioning = false;

    // Nombre del estado actual (ej: "Login", "Play"...). Lo usa TitleNotificationUI
    // para no mostrar notificaciones mientras se esta en LoginState.
    public string CurrentStateName { get; private set; }

    private void Start()
    {
        if (isTransitioning) return;

        if (AuthenticationService.Instance.IsSignedIn)
        { 
            // Ya hay sesion (p. ej. al volver al menu tras completar un nivel): es seguro
            // lanzar los checks de recompensa. En arranque en frio el usuario aun no esta
            // logueado -> no entra aqui; lo hara PlayerProgressManager.OnUserSignedIn() tras login.
            // Los checks son idempotentes, asi que no hay riesgo de notificar dos veces.
            if (TitleNotificationManager.Instance != null)
                TitleNotificationManager.Instance.RunPostLoginChecks();

            string targetState = PlayerPrefs.GetString("TargetMainState", "Play");
            PlayerPrefs.DeleteKey("TargetMainState");
            ChangeState(targetState); 
        }
        else 
        {
            ChangeState("Login"); 
        }
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
            Debug.LogError($"No se encontro el prefab en Resources/UI/{stateName}UI");
            yield break;
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

            // Animar transicion
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

            // Terminar transicion
            currentCG.alpha = 0;
            nextCG.alpha = 1;
            currentRT.anchoredPosition = new Vector2(-slideDistance, 0);
            nextRT.anchoredPosition = Vector2.zero;

            // Destruir la pantalla anterior
            currentState.OnExit();
            Destroy(currentState.gameObject);
        }

        currentState = nextState;
        CurrentStateName = stateName;
        nextState.OnEnter();
        isTransitioning = false;
    }
}
