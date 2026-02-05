using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_AvatarPicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform gridContent;
    [SerializeField] private GameObject avatarSlotPrefab;

    // Datos internos
    private ProfileAvatarSO[] allAvatarsData;
    private System.Action onAvatarChangedCallback; // Para avisar al padre de que hemos acabado

    private void Awake()
    {
        // Carga sus propios datos. Es independiente.
        allAvatarsData = Resources.LoadAll<ProfileAvatarSO>("Avatars");

        closeButton.onClick.AddListener(Close);
    }

    // Esta función la llama el ProfileState para abrir la ventana
    public void Show(System.Action onClosed)
    {
        this.onAvatarChangedCallback = onClosed;
        gameObject.SetActive(true);
        GenerateGrid();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void GenerateGrid()
    {
        // 1. Limpiar lo viejo
        foreach (Transform child in gridContent) Destroy(child.gameObject);

        var progress = PlayerProgressManager.Instance;
        string currentId = progress.GetEquippedAvatarId();

        // 2. Crear fichas
        foreach (var avData in allAvatarsData)
        {
            var slotGO = Instantiate(avatarSlotPrefab, gridContent);
            var slotScript = slotGO.GetComponent<UI_AvatarSlot>();

            // Lógica de desbloqueo
            bool isUnlocked = avData.isFree;
            if (!isUnlocked && !string.IsNullOrEmpty(avData.requiredAchievementId))
            {
                isUnlocked = progress.HasUnlocked(avData.requiredAchievementId);
            }

            bool isSelected = avData.id == currentId;

            // Al hacer clic...
            slotScript.Setup(avData, isUnlocked, isSelected, (clickedId) =>
            {
                // 1. Guardamos
                progress.EquipAvatar(clickedId);
                // 2. Cerramos
                Close();
                // 3. Avisamos al padre (ProfileState) para que actualice la foto grande
                onAvatarChangedCallback?.Invoke();
            });
        }
    }
}