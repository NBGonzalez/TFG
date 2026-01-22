using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    private PlayerProgress progress; // Básicamente esto es un diccionario.

    private const string CLOUD_KEY = "player_progress";
    private string SavePath => Path.Combine(Application.persistentDataPath, "player_progress.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocal();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // PATRÓN OBSERVADOR: Nos suscribimos al evento de "Usuario Logueado".
        // Esto se ejecuta automáticamente cuando el LoginManager tiene éxito.
        // 1. Nos suscribimos para futuros eventos
        AuthenticationService.Instance.SignedIn += OnUserSignedIn;

        // 2. COMPROBACIÓN EXTRA:
        // ¿Y si ya estábamos logueados de antes (por ejemplo, reinicio rápido)?
        // Comprobamos el estado actual por si acaso nos perdimos el evento.
        if (AuthenticationService.Instance.IsSignedIn)
        {
            OnUserSignedIn();
        }

        //File.Delete(SavePath);
        //Debug.Log("Progreso borrado para pruebas");
    }
    private void OnDestroy()
    {
        
        AuthenticationService.Instance.SignedIn -= OnUserSignedIn;
    }
    private async void OnUserSignedIn()
    {
        Debug.Log("[Progress] Usuario detectado. Intentando descargar datos de la nube...");
        await LoadFromCloudAsync();
    }


    // =============================
    //  PUBLIC API
    // =============================

    public bool IsLevelCompleted(string language, string levelId)
    {
        if (progress.completedLevels.ContainsKey(language))
        {
            return progress.completedLevels[language].Contains(levelId);
        }
        return false;
    }

    public async void CompleteLevel(string language, string levelId)
    {
        //if (progress == null) progress = new PlayerProgress();
        Debug.Log($"[PlayerProgressManager] Marcando nivel como completado: {language} - {levelId}");
        if (!progress.completedLevels.ContainsKey(language))
            progress.completedLevels[language] = new List<string>();

        if (!progress.completedLevels[language].Contains(levelId))
        {
            Debug.Log($"[PlayerProgressManager] Nivel añadido a la lista de completados.");
            progress.completedLevels[language].Add(levelId);
            SaveLocal();
            await SaveToCloudAsync();
        }
    }

    public List<string> GetCompletedLevels(string language)
    {
        if (!progress.completedLevels.ContainsKey(language))
            return new List<string>();

        return new List<string>(progress.completedLevels[language]);
    }

    // =============================
    // SAVE / LOAD
    // =============================
    private async Task SaveToCloudAsync()
    {
        try
        {
            // Preparamos los datos. Cloud Save guarda Diccionarios string -> object
            var dataToSave = new Dictionary<string, object> { { CLOUD_KEY, progress } };

            // ¡Llamada a la nube!
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);

            Debug.Log("<color=green>[Cloud] Progreso guardado en la nube con éxito.</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cloud] Error al guardar en la nube: {e.Message}");
            // Aquí no pasa nada grave, el usuario tiene su copia local.
        }
    }

    private async Task LoadFromCloudAsync()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { CLOUD_KEY });

            if (savedData.TryGetValue(CLOUD_KEY, out var cloudItem))
            {
                // 1. Obtenemos lo que hay en la nube (ej: Nivel 1)
                PlayerProgress cloudProgress = cloudItem.Value.GetAs<PlayerProgress>();

                // 2. FUSIÓN (MERGE): 
                // En lugar de machacar 'progress', vamos a mezclar lo de la nube con lo local.
                // Si 'progress' es null (primera vez), usamos el de la nube directamente.
                if (progress == null)
                {
                    progress = cloudProgress;
                }
                else
                {
                    // Recorremos los lenguajes que vienen de la nube
                    foreach (var langEntry in cloudProgress.completedLevels)
                    {
                        string language = langEntry.Key;       // Ej: "SQL"
                        List<string> levels = langEntry.Value; // Ej: ["sql-1"]

                        // Aseguramos que existe la lista en local
                        if (!progress.completedLevels.ContainsKey(language))
                        {
                            progress.completedLevels[language] = new List<string>();
                        }

                        // Añadimos solo los niveles que NO tengamos ya
                        foreach (var lvl in levels)
                        {
                            if (!progress.completedLevels[language].Contains(lvl))
                            {
                                progress.completedLevels[language].Add(lvl);
                                Debug.Log($"[Sync] Recuperado nivel {lvl} de la nube.");
                            }
                        }
                    }
                }

                // 3. PASO CRÍTICO: 
                // Ahora 'progress' tiene LA SUMA de (Lo que hice en el metro + Lo que había en la nube).
                // Tenemos que subir esta versión "definitiva" a la nube para que se entere de lo del metro.

                Debug.Log("<color=cyan>[Cloud] Sincronización completada (Fusión Local+Nube).</color>");

                // Guardamos la fusión en ambos lados
                SaveLocal();
                await SaveToCloudAsync();
            }
            else
            {
                Debug.Log("[Cloud] Usuario nuevo o sin datos en nube.");
                // Si no hay nada en la nube, pero yo tengo cosas en local (del metro),
                // las subo ahora mismo.
                if (progress != null && progress.completedLevels.Count > 0)
                {
                    await SaveToCloudAsync();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Cloud] Error al cargar/fusionar: {e.Message}");
        }
    }

    private void SaveLocal()
    {
        string json = JsonConvert.SerializeObject(progress, Formatting.Indented);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Progress] Guardado en: {SavePath}\n{json}");
    }

    private void LoadLocal()
    {
        if (!File.Exists(SavePath))
        {
            progress = new PlayerProgress();
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            progress = JsonConvert.DeserializeObject<PlayerProgress>(json) ?? new PlayerProgress();

            if (progress.completedLevels == null)
                progress.completedLevels = new Dictionary<string, List<string>>();

            Debug.Log("[Local] Progreso cargado del disco.");
        }
        catch
        {
            progress = new PlayerProgress();
        }
    }
}
