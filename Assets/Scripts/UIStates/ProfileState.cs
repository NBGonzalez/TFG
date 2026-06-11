//ProfileState.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileState : UIStateBase
{
    [Header("--- General ---")]
    [SerializeField] private Button backButton;

    [Header("--- Cabecera ---")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI currentTitleText;
    [SerializeField] private Image currentAvatarImage;
    [SerializeField] private Button editAvatarButton;

    [Header("--- Stats ---")]
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private TextMeshProUGUI totalStarsText;

    [Header("--- Titulos ---")]
    [SerializeField] private Transform titlesGridContent;
    [SerializeField] private GameObject titleSlotPrefab;
    [SerializeField] private TextMeshProUGUI unlockedTitlesCountText;

    [Header("--- Panel Info Titulo Bloqueado ---")]
    [SerializeField] private GameObject lockedTitlePanel;
    [SerializeField] private Image lockedTitleImage;
    [SerializeField] private TextMeshProUGUI lockedTitleNameText;
    [SerializeField] private TextMeshProUGUI lockedTitleDescription;
    [SerializeField] private Button lockedTitleOkButton;
    [SerializeField] private Button lockedTitleBackgroundButton; // Boton transparente del fondo para cerrar al tocar fuera

    [Header("--- Modulos ---")]
    [SerializeField] private UI_AvatarPicker avatarPickerModule;

    private ProfileTitleSO[] allTitlesData;
    private ProfileAvatarSO[] allAvatarsData;

    public override void OnEnter()
    {
        base.OnEnter();

        allTitlesData = Resources.LoadAll<ProfileTitleSO>("Titles");
        allAvatarsData = Resources.LoadAll<ProfileAvatarSO>("Avatars");

        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));

        editAvatarButton.onClick.AddListener(() =>
        {
            avatarPickerModule.Show(
                onClosed: () => RefreshUI(),
                onLockedClick: (lockedAvatar) => ShowLockedAvatarPanel(lockedAvatar)
            );
        });

        // Configurar botones del panel de titulo bloqueado
        if (lockedTitleOkButton != null)
            lockedTitleOkButton.onClick.AddListener(CloseLockedTitlePanel);

        if (lockedTitleBackgroundButton != null)
            lockedTitleBackgroundButton.onClick.AddListener(CloseLockedTitlePanel);

        // Asegurarse de que el panel empieza cerrado
        if (lockedTitlePanel != null)
            lockedTitlePanel.SetActive(false);

        RefreshUI();
    }

    private void RefreshUI()
    {
        var progress = PlayerProgressManager.Instance;
        if (progress == null) return;

        if (GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated())
            nameText.text = GooglePlayGames.PlayGamesPlatform.Instance.GetUserDisplayName();
        else
            nameText.text = "Invitado";

        streakText.text = progress.GetStreak().ToString();
        totalStarsText.text = progress.GetTotalStars().ToString();

        string equippedTitleId = progress.GetEquippedTitle();
        var titleData = GetTitleDataById(equippedTitleId);
        currentTitleText.text = titleData != null ? titleData.titleName : "Novato";
        if (titleData != null) currentTitleText.color = titleData.titleColor;

        string equippedAvatarId = progress.GetEquippedAvatarId();
        var avatarData = GetAvatarDataById(equippedAvatarId);
        if (avatarData != null) currentAvatarImage.sprite = avatarData.avatarImage;

        GenerateAchievementsGrid(progress);
    }

    private ProfileTitleSO GetTitleDataById(string id)
    {
        foreach (var t in allTitlesData) if (t.id == id) return t;
        return null;
    }

    private ProfileAvatarSO GetAvatarDataById(string id)
    {
        foreach (var a in allAvatarsData) if (a.id == id) return a;
        return null;
    }

    private bool IsTitleUnlocked(ProfileTitleSO titleData, PlayerProgressManager progress)
    {
        if (progress.HasUnlocked(titleData.id)) return true;

        bool unlocked = false;
        switch (titleData.requirementType)
        {
            case UnlockRequirementType.None:
                unlocked = true;
                break;
            case UnlockRequirementType.TotalStars:
                unlocked = progress.GetTotalStars() >= titleData.requirementValue;
                break;
            case UnlockRequirementType.StreakDays:
                unlocked = progress.GetStreak() >= titleData.requirementValue;
                break;
            case UnlockRequirementType.SQLLevelPassed:
                unlocked = progress.IsLevelCompleted("SQL", $"sql-{titleData.requirementValue}");
                break;
            case UnlockRequirementType.BiologiaLevelPassed:
                unlocked = progress.IsLevelCompleted("Biologia", $"biologia-{titleData.requirementValue}");
                break;
        }

        if (unlocked) progress.UnlockAchievement(titleData.id);
        return unlocked;
    }

    private void GenerateAchievementsGrid(PlayerProgressManager progress)
    {
        foreach (Transform child in titlesGridContent) Destroy(child.gameObject);

        int unlockedCount = 0;

        // Ordenar: 1) Desbloqueados primero, 2) Por tipo de requisito, 3) Por valor ascendente
        System.Array.Sort(allTitlesData, (a, b) =>
        {
            bool aUnlocked = IsTitleUnlocked(a, progress);
            bool bUnlocked = IsTitleUnlocked(b, progress);

            // 1. Desbloqueados antes que bloqueados
            if (aUnlocked != bUnlocked)
                return bUnlocked.CompareTo(aUnlocked);

            // 2. Agrupar por tipo de requisito (orden del enum)
            if (a.requirementType != b.requirementType)
                return a.requirementType.CompareTo(b.requirementType);

            // 3. Dentro del mismo tipo, ordenar por valor ascendente
            return a.requirementValue.CompareTo(b.requirementValue);
        });

        string equippedTitleId = progress.GetEquippedTitle();

        foreach (var titleData in allTitlesData)
        {
            bool isUnlocked = IsTitleUnlocked(titleData, progress);

            if (isUnlocked) unlockedCount++;

            GameObject newSlot = Instantiate(titleSlotPrefab, titlesGridContent);
            UI_TitleSlot slotScript = newSlot.GetComponent<UI_TitleSlot>();

            bool isEquipped = titleData.id == equippedTitleId;

            // Pasamos ambos callbacks: equipar (desbloqueado) y mostrar info (bloqueado)
            slotScript.Setup(titleData, isUnlocked,
                onClickEquip: (id) =>
                {
                    progress.EquipTitle(id);
                    RefreshUI();
                },
                onClickLocked: (data) =>
                {
                    ShowLockedTitlePanel(data);
                },
                isEquipped: isEquipped
            );
        }
        unlockedTitlesCountText.text = $"Titulos Desbloqueados: {unlockedCount}/{allTitlesData.Length}";
    }

    // =========================================
    //  PANEL DE TITULO BLOQUEADO
    // =========================================
    private void ShowLockedTitlePanel(ProfileTitleSO data)
    {
        if (lockedTitlePanel == null) return;

        // Rellenar el nombre, la imagen y la descripcion del titulo
        if (lockedTitleImage != null) lockedTitleImage.sprite = data.icon;
        if (lockedTitleNameText != null) lockedTitleNameText.text = data.titleName;
        if (lockedTitleDescription != null) lockedTitleDescription.text = data.description;

        lockedTitlePanel.SetActive(true);
    }

    private void ShowLockedAvatarPanel(ProfileAvatarSO avatarData)
    {
        if (lockedTitlePanel == null) return;

        // Mostrar la imagen del avatar
        if (lockedTitleImage != null) lockedTitleImage.sprite = avatarData.avatarImage;

        // Buscar el titulo asociado para obtener nombre y descripcion
        ProfileTitleSO associatedTitle = null;
        if (!string.IsNullOrEmpty(avatarData.requiredAchievementId))
        {
            associatedTitle = GetTitleDataById(avatarData.requiredAchievementId);
        }

        if (associatedTitle != null)
        {
            if (lockedTitleNameText != null) lockedTitleNameText.text = associatedTitle.titleName;
            if (lockedTitleDescription != null) lockedTitleDescription.text = associatedTitle.description;
        }
        else
        {
            if (lockedTitleNameText != null) lockedTitleNameText.text = "Bloqueado";
            if (lockedTitleDescription != null) lockedTitleDescription.text = "Consigue el logro necesario para desbloquear este avatar.";
        }

        lockedTitlePanel.SetActive(true);
    }

    private void CloseLockedTitlePanel()
    {
        if (lockedTitlePanel != null)
            lockedTitlePanel.SetActive(false);
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
        editAvatarButton.onClick.RemoveAllListeners();

        if (lockedTitleOkButton != null)
            lockedTitleOkButton.onClick.RemoveAllListeners();

        if (lockedTitleBackgroundButton != null)
            lockedTitleBackgroundButton.onClick.RemoveAllListeners();

        CloseLockedTitlePanel();
    }
}