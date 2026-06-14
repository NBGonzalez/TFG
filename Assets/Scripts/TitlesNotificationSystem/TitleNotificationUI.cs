// TitleNotificationUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TitleNotificationUI : MonoBehaviour
{
    [Header("Referencias UI del Popup")]
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Image notificationIcon;
    [SerializeField] private TextMeshProUGUI defaultText;    // Texto superior (ej: "¡Has desbloqueado!")
    [SerializeField] private TextMeshProUGUI titleText;      // Texto inferior (ej: nombre del titulo)
    [SerializeField] private Button popupButton;

    [Header("Navegacion")]
    [SerializeField] private UIStateManager stateManager;

    [Header("Configuracion de Animacion")]
    [SerializeField] private float slideDistance = 200f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float displayDuration = 3.5f;

    private Coroutine currentNotification;
    private bool isShowing = false;
    private string currentTargetState; // Estado al que navegar si el jugador pulsa

    private void Start()
    {
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.blocksRaycasts = false;
        }

        if (popupButton != null)
        {
            popupButton.onClick.AddListener(OnPopupClicked);
        }
    }

    private void Update()
    {
        if (!isShowing && TitleNotificationManager.Instance != null && TitleNotificationManager.Instance.HasPending)
        {
            if (TitleNotificationManager.Instance.TryDequeue(out NotificationData data))
            {
                ShowNotification(data);
            }
        }
    }

    private void ShowNotification(NotificationData data)
    {
        if (currentNotification != null)
            StopCoroutine(currentNotification);

        currentNotification = StartCoroutine(NotificationRoutine(data));
    }

    private IEnumerator NotificationRoutine(NotificationData data)
    {
        isShowing = true;
        currentTargetState = data.targetState;

        // Rellenar datos del popup
        if (notificationIcon != null && data.icon != null) notificationIcon.sprite = data.icon;
        if (defaultText != null) defaultText.text = data.defaultText;
        if (titleText != null) titleText.text = data.titleText;

        // Posicion inicial: fuera de pantalla (arriba)
        Vector2 hiddenPos = new Vector2(popupRect.anchoredPosition.x, slideDistance);
        Vector2 visiblePos = new Vector2(popupRect.anchoredPosition.x, 0f);
        popupRect.anchoredPosition = hiddenPos;
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.blocksRaycasts = true;

        // --- SLIDE DOWN + FADE IN ---
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.SmoothStep(0f, 1f, t / slideDuration);
            popupRect.anchoredPosition = Vector2.Lerp(hiddenPos, visiblePos, lerp);
            popupCanvasGroup.alpha = lerp;
            yield return null;
        }
        popupRect.anchoredPosition = visiblePos;
        popupCanvasGroup.alpha = 1f;

        // --- ESPERAR ---
        yield return new WaitForSeconds(displayDuration);

        // --- SLIDE UP + FADE OUT ---
        t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.SmoothStep(0f, 1f, t / slideDuration);
            popupRect.anchoredPosition = Vector2.Lerp(visiblePos, hiddenPos, lerp);
            popupCanvasGroup.alpha = 1f - lerp;
            yield return null;
        }
        popupRect.anchoredPosition = hiddenPos;
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.blocksRaycasts = false;

        isShowing = false;
        currentNotification = null;
        currentTargetState = null;
    }

    private void OnPopupClicked()
    {
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }

        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.blocksRaycasts = false;
        isShowing = false;

        // Navegar al estado correspondiente (si hay uno definido)
        if (stateManager != null && !string.IsNullOrEmpty(currentTargetState))
        {
            stateManager.ChangeState(currentTargetState);
        }

        currentTargetState = null;
    }

    private void OnDestroy()
    {
        if (popupButton != null)
            popupButton.onClick.RemoveAllListeners();
    }
}