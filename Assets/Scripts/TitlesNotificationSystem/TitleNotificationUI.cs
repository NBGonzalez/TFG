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
    [SerializeField] private Image titleIcon;
    [SerializeField] private TextMeshProUGUI titleNameText;
    [SerializeField] private Button popupButton;

    [Header("Navegacion")]
    [SerializeField] private UIStateManager stateManager;

    [Header("Configuracion de Animacion")]
    [SerializeField] private float slideDistance = 200f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float displayDuration = 3.5f;

    private Coroutine currentNotification;
    private bool isShowing = false;

    private void Start()
    {
        // Asegurar que el popup empieza oculto
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.blocksRaycasts = false;
        }

        // Configurar el boton del popup
        if (popupButton != null)
        {
            popupButton.onClick.AddListener(OnPopupClicked);
        }
    }

    private void Update()
    {
        // Comprobar si hay notificaciones pendientes y no se esta mostrando ninguna
        if (!isShowing && TitleNotificationManager.Instance != null && TitleNotificationManager.Instance.HasPending)
        {
            if (TitleNotificationManager.Instance.TryDequeue(out ProfileTitleSO titleData))
            {
                ShowNotification(titleData);
            }
        }
    }

    private void ShowNotification(ProfileTitleSO data)
    {
        if (currentNotification != null)
            StopCoroutine(currentNotification);

        currentNotification = StartCoroutine(NotificationRoutine(data));
    }

    private IEnumerator NotificationRoutine(ProfileTitleSO data)
    {
        isShowing = true;

        // Rellenar datos del popup
        if (titleIcon != null) titleIcon.sprite = data.icon;
        if (titleNameText != null) titleNameText.text = data.titleName;

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
    }

    private void OnPopupClicked()
    {
        // Parar la animacion actual
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
            currentNotification = null;
        }

        // Ocultar el popup inmediatamente
        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.blocksRaycasts = false;
        isShowing = false;

        // Navegar al ProfileState
        if (stateManager != null)
        {
            stateManager.ChangeState("Profile");
        }
    }

    private void OnDestroy()
    {
        if (popupButton != null)
            popupButton.onClick.RemoveAllListeners();
    }
}