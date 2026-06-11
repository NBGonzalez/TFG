// AppColorManager.cs
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public struct ColorPalette
{
    public string name;
    public Color primaryColor;
    public Color secondaryColor;
    public Color lightColor;

    public Color buttonColor1;
    public Color buttonColor2;
    public Color buttonShinnyColor;
}

public class AppColorManager : MonoBehaviour
{
    public static AppColorManager Instance { get; private set; }
    public static event Action OnPaletteChanged;

    [Header("Paletas de la aplicacion")]
    [Tooltip("Rellena cada elemento con un color primario y secundario.")]
    public ColorPalette[] palettes;

    [Header("Fondo")]
    [Tooltip("Arrastra aqui el material BackGround2")]
    public Material backgroundMaterial;

    [Tooltip("Arrastra aqui la Image del Canvas que usa el material BackGround2")]
    public Graphic backgroundGraphic;

    [Header("Botones")]
    [Tooltip("Arrastra aqui el material de los botones")]
    public Material buttonsMaterial;

    [Header("PlayState")]
    [Tooltip("Color del fondo en el PlayState")]
    public Material backgroundPlayStateShader;


    // =========================================
    //  PRESETS DE DALTONISMO (definidos en codigo)
    // =========================================
    public struct ColorBlindPreset
    {
        public string name;
        public Color correctColor;
        public Color incorrectColor;
    }

    private readonly ColorBlindPreset[] colorBlindPresets = new ColorBlindPreset[]
    {
        new ColorBlindPreset { name = "Por Defecto", correctColor = HexColor("#00FF00"), incorrectColor = HexColor("#FF0000") },
        new ColorBlindPreset { name = "Protan/Deutan", correctColor = HexColor("#00FFE6"), incorrectColor = HexColor("#FF6600") },
        new ColorBlindPreset { name = "Tritan", correctColor = HexColor("#00FFC4"), incorrectColor = HexColor("#FF008B") },
        new ColorBlindPreset { name = "Alto Contraste", correctColor = HexColor("#FFE326"), incorrectColor = HexColor("#FFFFFF") },
    };

    // Estado actual
    private int currentPaletteIndex = 0;
    private int currentColorBlindIndex = 0;

    // =========================================
    //  API PUBLICA
    // =========================================
    public Color CorrectColor => colorBlindPresets[currentColorBlindIndex].correctColor;
    public Color IncorrectColor => colorBlindPresets[currentColorBlindIndex].incorrectColor;
    public int CurrentPaletteIndex => currentPaletteIndex;
    public int CurrentColorBlindIndex => currentColorBlindIndex;
    public int ColorBlindPresetCount => colorBlindPresets.Length;

    public ColorBlindPreset GetColorBlindPreset(int index)
    {
        if (index < 0 || index >= colorBlindPresets.Length) return colorBlindPresets[0];
        return colorBlindPresets[index];
    }

    // =========================================
    //  LIFECYCLE
    // =========================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadPreferences()
    {
        currentPaletteIndex = PlayerPrefs.GetInt("AppPaletteIndex", 0);
        currentColorBlindIndex = PlayerPrefs.GetInt("AppColorBlindIndex", 0);

        // Clamp para evitar indices fuera de rango
        if (palettes != null && palettes.Length > 0)
            currentPaletteIndex = Mathf.Clamp(currentPaletteIndex, 0, palettes.Length - 1);
        else
            currentPaletteIndex = 0;

        currentColorBlindIndex = Mathf.Clamp(currentColorBlindIndex, 0, colorBlindPresets.Length - 1);

        ApplyPalette();
    }

    // =========================================
    //  CAMBIAR PALETA
    // =========================================
    public void SetPalette(int index)
    {
        if (palettes == null || index < 0 || index >= palettes.Length) return;
        currentPaletteIndex = index;
        PlayerPrefs.SetInt("AppPaletteIndex", index);
        PlayerPrefs.Save();
        ApplyPalette();
    }

    private void ApplyPalette()
    {
        if (palettes == null || palettes.Length == 0) return;
        if (currentPaletteIndex >= palettes.Length) return;

        var palette = palettes[currentPaletteIndex];

        // Fondo principal
        if (backgroundMaterial != null)
        {

            backgroundMaterial.SetColor("_ColorBase", palette.primaryColor);
            backgroundMaterial.SetColor("_ColorEmision", palette.secondaryColor);
            backgroundMaterial.SetColor("_ColorLight", palette.lightColor);
        }

        // Fondo del graphic
        if (backgroundGraphic != null && backgroundGraphic.material != null)
        {
            backgroundGraphic.material.SetColor("_ColorBase", palette.primaryColor);
            backgroundGraphic.material.SetColor("_ColorEmision", palette.secondaryColor);
            backgroundGraphic.material.SetColor("_ColorLight", palette.lightColor);
        }

        // Botones (material compartido)
        if (buttonsMaterial != null)
        {
            buttonsMaterial.SetColor("_Color1", palette.buttonColor1);
            buttonsMaterial.SetColor("_Color2", palette.buttonColor2);
            buttonsMaterial.SetColor("_ColorShy", palette.buttonShinnyColor);
        }

        // Fondo del PlayState
        if (backgroundPlayStateShader != null)
        {
            backgroundPlayStateShader.SetColor("_BackGroundColor", palette.primaryColor);
        }

        // Avisar a todos los UIColorElement suscritos
        OnPaletteChanged?.Invoke();
    }

    // =========================================
    //  CAMBIAR MODO DALTONISMO
    // =========================================
    public void SetColorBlindMode(int index)
    {
        if (index < 0 || index >= colorBlindPresets.Length) return;
        currentColorBlindIndex = index;
        PlayerPrefs.SetInt("AppColorBlindIndex", index);
        PlayerPrefs.Save();
    }

    // =========================================
    //  UTILIDAD
    // =========================================
    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    // =========================================
    //  GETTERS DE COLORES ACTUALES
    // =========================================
    public Color GetPrimaryColor() { return palettes[currentPaletteIndex].primaryColor; }
    public Color GetSecondaryColor() { return palettes[currentPaletteIndex].secondaryColor; }
    public Color GetLightColor() { return palettes[currentPaletteIndex].lightColor; }

    public Color GetButtonColor1() { return palettes[currentPaletteIndex].buttonColor1; }
    public Color GetButtonColor2() { return palettes[currentPaletteIndex].buttonColor2; }
    public Color GetButtonShinyColor() { return palettes[currentPaletteIndex].buttonShinnyColor; }
}
