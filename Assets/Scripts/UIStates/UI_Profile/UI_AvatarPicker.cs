//UI_AvatarPicker.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_AvatarPicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform verticalLayoutContent; // Content del ScrollView con VerticalLayoutGroup
    [SerializeField] private GameObject avatarSlotPrefab;
    [SerializeField] private GameObject categoryPrefab; // Prefab con UI_AvatarCategory (titulo + grid)
    [SerializeField] private TextMeshProUGUI unlockedAvatarsCountText;

    // Datos internos
    private ProfileAvatarSO[] allAvatarsData;
    private ProfileTitleSO[] allTitlesData;
    private System.Action onAvatarChangedCallback;
    private System.Action<ProfileAvatarSO> onLockedAvatarCallback;

    private void Awake()
    {
        allAvatarsData = Resources.LoadAll<ProfileAvatarSO>("Avatars");
        allTitlesData = Resources.LoadAll<ProfileTitleSO>("Titles");
        closeButton.onClick.AddListener(Close);
    }

    public void Show(System.Action onClosed, System.Action<ProfileAvatarSO> onLockedClick = null)
    {
        this.onAvatarChangedCallback = onClosed;
        this.onLockedAvatarCallback = onLockedClick;
        gameObject.SetActive(true);
        GenerateGroupedGrid();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Devuelve un nombre legible para cada UnlockRequirementType.
    /// Si en el futuro se anaden mas tipos al enum, basta con anadir un caso aqui.
    /// </summary>
    private string GetCategoryName(UnlockRequirementType type)
    {
        return type switch
        {
            UnlockRequirementType.None        => "General",
            UnlockRequirementType.TotalStars  => "Estrellas",
            UnlockRequirementType.StreakDays   => "Racha Diaria",
            UnlockRequirementType.SQLLevelPassed     => "SQL",
            UnlockRequirementType.BiologiaLevelPassed => "Biologia",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Busca el ProfileTitleSO asociado a un avatar por su requiredAchievementId.
    /// </summary>
    private ProfileTitleSO FindAssociatedTitle(ProfileAvatarSO avatar)
    {
        if (string.IsNullOrEmpty(avatar.requiredAchievementId)) return null;
        foreach (var t in allTitlesData)
        {
            if (t.id == avatar.requiredAchievementId) return t;
        }
        return null;
    }

    private void GenerateGroupedGrid()
    {
        // 1. Limpiar todo lo anterior
        foreach (Transform child in verticalLayoutContent) Destroy(child.gameObject);

        var progress = PlayerProgressManager.Instance;
        string currentId = progress.GetEquippedAvatarId();
        int unlockedCount = 0;
        int totalCount = allAvatarsData.Length;

        // 2. Clasificar los avatares por categoria
        //    Usamos un int como clave: -1 = "Gratis", y el resto es el valor del enum
        var grouped = new SortedDictionary<int, List<ProfileAvatarSO>>();

        foreach (var avData in allAvatarsData)
        {
            int categoryKey;

            if (avData.isFree)
            {
                categoryKey = -1; // Grupo especial "Gratis" (aparece primero)
            }
            else
            {
                var title = FindAssociatedTitle(avData);
                categoryKey = title != null ? (int)title.requirementType : -1;
            }

            if (!grouped.ContainsKey(categoryKey))
                grouped[categoryKey] = new List<ProfileAvatarSO>();

            grouped[categoryKey].Add(avData);
        }

        // 3. Instanciar una categoria por cada grupo
        foreach (var kvp in grouped)
        {
            int key = kvp.Key;
            List<ProfileAvatarSO> avatarsInGroup = kvp.Value;

            // Determinar nombre de la categoria
            string categoryName;
            if (key == -1)
                categoryName = "Gratis";
            else
                categoryName = GetCategoryName((UnlockRequirementType)key);

            // Calcular desbloqueados en esta categoria
            int unlockedInCategory = 0;
            foreach (var avData in avatarsInGroup)
            {
                bool unlocked = avData.isFree;
                if (!unlocked && !string.IsNullOrEmpty(avData.requiredAchievementId))
                    unlocked = progress.HasUnlocked(avData.requiredAchievementId);
                if (unlocked) unlockedInCategory++;
            }

            // Instanciar el prefab de categoria con el contador
            GameObject categoryGO = Instantiate(categoryPrefab, verticalLayoutContent);
            var categoryScript = categoryGO.GetComponent<UI_AvatarCategory>();
            categoryScript.Setup(categoryName, unlockedInCategory, avatarsInGroup.Count);

            // 4. Instanciar los avatar slots dentro del grid de esta categoria
            foreach (var avData in avatarsInGroup)
            {
                var slotGO = Instantiate(avatarSlotPrefab, categoryScript.GridContent);
                var slotScript = slotGO.GetComponent<UI_AvatarSlot>();

                bool isUnlocked = avData.isFree;
                if (!isUnlocked && !string.IsNullOrEmpty(avData.requiredAchievementId))
                {
                    isUnlocked = progress.HasUnlocked(avData.requiredAchievementId);
                }

                if (isUnlocked) unlockedCount++;

                bool isSelected = avData.id == currentId;

                slotScript.Setup(avData, isUnlocked, isSelected,
                    onClick: (clickedId) =>
                    {
                        progress.EquipAvatar(clickedId);
                        Close();
                        onAvatarChangedCallback?.Invoke();
                    },
                    onClickLocked: (lockedAvatar) =>
                    {
                        onLockedAvatarCallback?.Invoke(lockedAvatar);
                    }
                );
            }
        }

        unlockedAvatarsCountText.text = $"Avatares Desbloqueados: {unlockedCount}/{totalCount}";
    }
}