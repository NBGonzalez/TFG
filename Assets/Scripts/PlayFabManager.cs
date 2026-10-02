using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    // ==========================================
    // EL PATRÓN SINGLETON
    // ==========================================
    public static PlayFabManager Instancia; // Esto nos permite acceder desde cualquier otro script

    // Aquí guardaremos los datos para cuando la UI nazca y los pida
    public Dictionary<string, string> pathsGuardados = new Dictionary<string, string>();
    public bool descargaCompletada = false;

    // --- MOCHILA DE 2 HUECOS ---
    private Dictionary<string, string> nivelesEnCache = new Dictionary<string, string>();
    private Queue<string> ordenCache = new Queue<string>(); // Controla quién entró primero
    private const int MAX_HUECOS = 2;

    void Awake()
    {
        // Configuramos el Singleton al arrancar
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        IniciarSesionInvisible();
    }

    // ==========================================
    // FASE 1: INICIO DE SESIÓN
    // ==========================================
    void IniciarSesionInvisible()
    {
        Debug.Log("Conectando con los servidores de PlayFab...");
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginExito, OnErrorGlobal);
    }

    void OnLoginExito(LoginResult result)
    {
        Debug.Log("¡Inicio de sesión exitoso! Ya estamos dentro de PlayFab.");
        DescargarIndiceDePaths();
    }

    // ==========================================
    // FASE 2: DESCARGAR EL ÍNDICE
    // ==========================================
    void DescargarIndiceDePaths()
    {
        Debug.Log("Pidiendo solo el índice de rutas...");
        var request = new GetTitleDataRequest { Keys = new List<string> { "indice_paths" } };
        PlayFabClientAPI.GetTitleData(request, OnIndiceRecibido, OnErrorGlobal);
    }

    void OnIndiceRecibido(GetTitleDataResult result)
    {
        if (result.Data.ContainsKey("indice_paths"))
        {
            string[] listaDeClaves = result.Data["indice_paths"].Split(',');
            DescargarPathsEspecificos(new List<string>(listaDeClaves));
        }
        else
        {
            Debug.LogWarning("¡Ojo! No he encontrado la clave 'indice_paths' en PlayFab.");
        }
    }

    // ==========================================
    // FASE 3: DESCARGAR LOS PATHS Y GUARDARLOS
    // ==========================================
    void DescargarPathsEspecificos(List<string> clavesQueQueremos)
    {
        Debug.Log("Descargando el contenido exacto de los paths...");
        var request = new GetTitleDataRequest { Keys = clavesQueQueremos };
        PlayFabClientAPI.GetTitleData(request, OnPathsRecibidos, OnErrorGlobal);
    }

    void OnPathsRecibidos(GetTitleDataResult result)
    {
        Debug.Log("¡Descarga finalizada! Guardando en la mochila pública...");

        // Llenamos nuestra mochila pública
        pathsGuardados.Clear();
        foreach (var elemento in result.Data)
        {
            pathsGuardados.Add(elemento.Key, elemento.Value);
        }

        descargaCompletada = true; // Avisamos de que ya hemos terminado

        // CASO ESPECIAL: Si por lo que sea el jugador entró a la pantalla de Play 
        // ANTES de que el internet terminara de descargar, le avisamos ahora:
        UI_PlayScreen pantallaUI = FindObjectOfType<UI_PlayScreen>();
        if (pantallaUI != null)
        {
            pantallaUI.RecibirDatosDeLaNube(pathsGuardados);
        }
    }

    // ==========================================
    // FASE 4: EL CEREBRO DE LOS NIVELES Y LA MOCHILA
    // ==========================================

    // Esta es la función principal que llamará tu GameSceneManager
    public void PedirNivel(string language, string levelId, Action<string> alTerminar)
    {
        // Mediciones de latencia:
        var cronometro = System.Diagnostics.Stopwatch.StartNew();

        // Construimos el nombre exacto que tiene en PlayFab (ej: "sql_sql-1")
        string clavePlayFab = $"{language.ToLower()}_{levelId.ToLower()}";
        //Debug.Log($"[PlayFabManager] Pidiendo nivel: {clavePlayFab}");

        // CASO A: ¡Ya lo tenemos en la mochila! (Cero tiempos de carga)
        if (nivelesEnCache.ContainsKey(clavePlayFab))
        {
            cronometro.Stop();
            Debug.Log($"[METRICA] cache;{clavePlayFab};{cronometro.ElapsedMilliseconds}");

            Debug.Log($"[PlayFabManager] ¡El nivel {clavePlayFab} ya estaba en RAM! Carga instantánea.");
            alTerminar?.Invoke(nivelesEnCache[clavePlayFab]);

            // Descargamos el siguiente por si acaso
            PreCargarSiguienteNivel(language, levelId);
            return;
        }

        // CASO B: No lo tenemos. Hay que pedírselo a internet.
        //Debug.Log($"[PlayFabManager] El nivel {clavePlayFab} no está en RAM. Descargando de la nube...");
        var request = new GetTitleDataRequest { Keys = new List<string> { clavePlayFab } };

        PlayFabClientAPI.GetTitleData(request, (result) =>
        {
            if (result.Data.ContainsKey(clavePlayFab))
            {
                string jsonDescargado = result.Data[clavePlayFab];
                GuardarEnMochila(clavePlayFab, jsonDescargado);

                // Mediciones de latencia:
                cronometro.Stop();
                Debug.Log($"[METRICA] nube;{clavePlayFab};{cronometro.ElapsedMilliseconds};{jsonDescargado.Length}");

                // Entregamos el nivel al juego
                alTerminar?.Invoke(jsonDescargado);

                // Y en la sombra, pre-cargamos el siguiente
                PreCargarSiguienteNivel(language, levelId);
            }
            else
            {
                Debug.LogError($"[PlayFabManager] No se encontró la clave {clavePlayFab} en PlayFab.");
                alTerminar?.Invoke(null);
            }
        }, OnErrorGlobal);
    }

    // --- LA MAGIA DEL PRE-FETCHING ---
    private void PreCargarSiguienteNivel(string language, string levelIdActual)
    {
        // levelId suele ser "sql-1". Vamos a cortarlo para sumarle 1 al número.
        string[] partes = levelIdActual.Split('-');
        if (partes.Length == 2 && int.TryParse(partes[1], out int numeroActual))
        {
            int numeroSiguiente = numeroActual + 1;
            string levelIdSiguiente = $"{partes[0]}-{numeroSiguiente}";
            string claveSiguiente = $"{language.ToLower()}_{levelIdSiguiente.ToLower()}";

            // Si ya está en la mochila, no hacemos nada
            if (nivelesEnCache.ContainsKey(claveSiguiente)) return;

            //Debug.Log($"[PlayFabManager] Pre-cargando en la sombra el siguiente nivel: {claveSiguiente}...");

            var request = new GetTitleDataRequest { Keys = new List<string> { claveSiguiente } };
            PlayFabClientAPI.GetTitleData(request, (result) =>
            {
                // Si el nivel existe (puede que el jugador esté en el último nivel y no haya más), lo guardamos
                if (result.Data.ContainsKey(claveSiguiente))
                {
                    //Debug.Log($"[PlayFabManager] Pre-carga exitosa. Nivel {claveSiguiente} guardado en la recámara.");
                    GuardarEnMochila(claveSiguiente, result.Data[claveSiguiente]);
                }
            }, (error) => { /* Silenciamos los errores de pre-carga para no asustar, ya que es en segundo plano */ });
        }
    }

    // --- EL CONTROL DE LA MOCHILA DE 2 HUECOS ---
    private void GuardarEnMochila(string clave, string json)
    {
        if (!nivelesEnCache.ContainsKey(clave))
        {
            // Si la mochila ya tiene 2 cosas, sacamos la más vieja
            if (ordenCache.Count >= MAX_HUECOS)
            {
                string claveVieja = ordenCache.Dequeue();
                nivelesEnCache.Remove(claveVieja);
                Debug.Log($"[PlayFabManager] Mochila llena. Borrando de RAM el nivel viejo: {claveVieja}");
            }

            // Metemos lo nuevo
            nivelesEnCache.Add(clave, json);
            ordenCache.Enqueue(clave);
        }
    }

    void OnErrorGlobal(PlayFabError error) // Renombré OnErrorDatos a OnErrorGlobal para unificar
    {
        Debug.LogError("Error en PlayFab: " + error.GenerateErrorReport());
    }
}