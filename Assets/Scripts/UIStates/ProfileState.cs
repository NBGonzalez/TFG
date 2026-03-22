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
    [SerializeField] private Button editAvatarButton; // El botón transparente sobre la foto

    [Header("--- Stats ---")]
    [SerializeField] private TextMeshProUGUI streakText;
    [SerializeField] private TextMeshProUGUI totalStarsText;

    [Header("--- Títulos ---")]
    [SerializeField] private Transform titlesGridContent;
    [SerializeField] private GameObject titleSlotPrefab;
    [SerializeField] private TextMeshProUGUI unlockedTitlesCountText;

    [Header("--- Módulos ---")]
    [SerializeField] private UI_AvatarPicker avatarPickerModule; // REFERENCIA CLAVE al nuevo script

    private ProfileTitleSO[] allTitlesData;
    private ProfileAvatarSO[] allAvatarsData; // Necesitamos esto para pintar la foto grande

    public override void OnEnter()
    {
        base.OnEnter();

        // Cargamos datos
        allTitlesData = Resources.LoadAll<ProfileTitleSO>("Titles");
        allAvatarsData = Resources.LoadAll<ProfileAvatarSO>("Avatars");

        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));

        // CONEXIÓN LIMPIA:
        // Al hacer clic, le decimos al módulo: "Muéstrate, y cuando acabes, avísame (RefreshUI)"
        editAvatarButton.onClick.AddListener(() =>
        {
            avatarPickerModule.Show(onClosed: () => RefreshUI());
        });

        RefreshUI();
    }

    private void RefreshUI()
    {
        var progress = PlayerProgressManager.Instance;
        if (progress == null) return;

        // Textos y Stats
        if (GooglePlayGames.PlayGamesPlatform.Instance.IsAuthenticated())
            nameText.text = GooglePlayGames.PlayGamesPlatform.Instance.GetUserDisplayName();
        else
            nameText.text = "Invitado";

        streakText.text = progress.GetStreak().ToString();
        totalStarsText.text = progress.GetTotalStars().ToString();

        // Título Equipado
        string equippedTitleId = progress.GetEquippedTitle();
        var titleData = GetTitleDataById(equippedTitleId);
        currentTitleText.text = titleData != null ? titleData.titleName : "Novato";
        if (titleData != null) currentTitleText.color = titleData.titleColor;

        // Avatar Equipado (NUEVO: Esto faltaba en tu script anterior)
        string equippedAvatarId = progress.GetEquippedAvatarId();
        var avatarData = GetAvatarDataById(equippedAvatarId);
        if (avatarData != null) currentAvatarImage.sprite = avatarData.avatarImage;

        // Lista de Títulos
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

    //private void GenerateAchievementsGrid(PlayerProgressManager progress)
    //{
    //    foreach (Transform child in titlesGridContent) Destroy(child.gameObject);
    //    foreach (var titleData in allTitlesData)
    //    {
    //        GameObject newSlot = Instantiate(titleSlotPrefab, titlesGridContent);
    //        UI_TitleSlot slotScript = newSlot.GetComponent<UI_TitleSlot>();

    //        // Usamos la función HasUnlocked que añadimos al Manager
    //        bool isUnlocked = titleData.id == "Novato" || progress.HasUnlocked(titleData.id);

    //        slotScript.Setup(titleData, isUnlocked, (id) => {
    //            progress.EquipTitle(id);
    //            RefreshUI();
    //        });
    //    }
    //}

    private void GenerateAchievementsGrid(PlayerProgressManager progress)
    {
        // Limpieza...
        foreach (Transform child in titlesGridContent) Destroy(child.gameObject);

        int unlockedCount = 0;

        foreach (var titleData in allTitlesData)
        {
            GameObject newSlot = Instantiate(titleSlotPrefab, titlesGridContent);
            UI_TitleSlot slotScript = newSlot.GetComponent<UI_TitleSlot>();

            // --- AQUÍ ESTÁ LA LÓGICA DESACOPLADA ---
            // El Manager no sabe nada. Es la Vista la que calcula.

            bool isUnlocked = false;

            // 1. ¿Ya lo tengo guardado en mi lista de desbloqueados?
            if (progress.HasUnlocked(titleData.id))
            {
                isUnlocked = true;
            }
            // 2. Si no, ¿cumplo los requisitos AHORA MISMO?
            else
            {
                switch (titleData.requirementType)
                {
                    case UnlockRequirementType.None:
                        isUnlocked = true;
                        break;
                    case UnlockRequirementType.TotalStars:
                        if (progress.GetTotalStars() >= titleData.requirementValue) isUnlocked = true;
                        break;
                    case UnlockRequirementType.StreakDays:
                        if (progress.GetStreak() >= titleData.requirementValue) isUnlocked = true;
                        break;
                }

                // 3. Si acabo de descubrir que lo cumplo, ¡lo guardo para siempre!
                if (isUnlocked)
                {
                    progress.UnlockAchievement(titleData.id);
                    // Aquí podríamos lanzar un sonido de "¡Nuevo Logro!"
                }

            }
            if (isUnlocked)
            {
                unlockedCount++; // Contamos este título como desbloqueado para mostrarlo en el texto
            }

            // Configuramos el slot visualmente
            slotScript.Setup(titleData, isUnlocked, (id) => {
                progress.EquipTitle(id);
                RefreshUI();
            });
        }
        unlockedTitlesCountText.text = $"Títulos Desbloqueados: {unlockedCount}/{allTitlesData.Length}";
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
        editAvatarButton.onClick.RemoveAllListeners();
    }
}