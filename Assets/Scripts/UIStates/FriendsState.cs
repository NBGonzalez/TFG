using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FriendsState : UIStateBase
{
    [Header("Referencias")]
    [SerializeField] private Button backButton;
    [SerializeField] private Transform listContent;       // El "Content" del ScrollView
    [SerializeField] private GameObject leaderboardSlotPrefab; // El prefab que acabas de crear
    [SerializeField] private GameObject loadingSpinner;   // (Opcional) Un texto o icono que diga "Cargando..."

    // Datos estáticos para "traducir" IDs a Imágenes
    private ProfileAvatarSO[] allAvatars;
    private ProfileTitleSO[] allTitles;

    public override void OnEnter()
    {
        base.OnEnter();
        backButton.onClick.AddListener(() => stateManager.ChangeState("Main"));

        // 1. Cargar base de datos visual (Resources)
        allAvatars = Resources.LoadAll<ProfileAvatarSO>("Avatars");
        allTitles = Resources.LoadAll<ProfileTitleSO>("Titles");

        // 2. Generar el Ranking
        RefreshLeaderboard();
    }

    // Es 'async' porque vamos a esperar datos simulados o de red
    private async void RefreshLeaderboard()
    {
        // Limpiar lista vieja
        foreach (Transform child in listContent) Destroy(child.gameObject);

        // Mostrar "Cargando..."
        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        // --- AQUÍ ELEGIMOS EL PROVEEDOR ---
        // Ahora usamos el Mock. En el futuro aquí pondremos: new GooglePlayProvider();
        ILeaderboardProvider provider = new MockLeaderboardProvider();

        // Pedimos datos (esperamos 0.5s simulados)
        List<LeaderboardEntry> data = await provider.GetRanking();

        // Ocultar "Cargando..."
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        // Pintar la lista
        foreach (var entry in data)
        {
            GameObject newSlot = Instantiate(leaderboardSlotPrefab, listContent);
            UI_LeaderboardSlot slotScript = newSlot.GetComponent<UI_LeaderboardSlot>();

            // Buscamos los ScriptableObjects visuales usando los IDs (string)
            ProfileAvatarSO avatarData = GetAvatarById(entry.avatarId);
            ProfileTitleSO titleData = GetTitleById(entry.titleId);

            slotScript.Setup(entry, avatarData, titleData);
        }
    }

    // --- Helpers de búsqueda ---
    private ProfileAvatarSO GetAvatarById(string id)
    {
        if (allAvatars == null) return null;
        foreach (var a in allAvatars) if (a.id == id) return a;
        return null; // O devolver un default
    }

    private ProfileTitleSO GetTitleById(string id)
    {
        if (allTitles == null) return null;
        foreach (var t in allTitles) if (t.id == id) return t;
        return null;
    }

    public override void OnExit()
    {
        backButton.onClick.RemoveAllListeners();
    }
}