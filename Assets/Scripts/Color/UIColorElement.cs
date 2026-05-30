// UIColorElement.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIColorElement : MonoBehaviour
{
    public enum ColorRole
    {
        Primary,
        Secondary,
        Light,
        ButtonColor1,
        ButtonColor2,
        ButtonShiny,
        Correct,
        Incorrect
    }

    [Header("Configuración de Color")]
    public ColorRole role = ColorRole.Secondary;

    [Header("Opciones Avanzadas")]
    public bool modifyButtonColorBlock = true;

    private float colorSaturation = 0.6f;

    private Image _image;
    private Button _button;
    private TextMeshProUGUI _text;
    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _text = GetComponent<TextMeshProUGUI>();
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        AppColorManager.OnPaletteChanged += UpdateVisualColor;
        UpdateVisualColor();
    }

    private void OnDisable()
    {
        AppColorManager.OnPaletteChanged -= UpdateVisualColor;
    }

    /// <summary>
    /// Fuerza el alpha de un color a 1 para garantizar visibilidad.
    /// </summary>
    private static Color Opaque(Color c)
    {
        c.a = 1f;
        return c;
    }

    public void UpdateVisualColor()
    {
        // Seguro de vida: Si ocurre un error aquí dentro, lo capturamos para no congelar la UI
        try
        {
            if (AppColorManager.Instance == null) return;

            Color targetColor = Opaque(GetColorFromRole(role));

            if (_dropdown != null)
            {
                ApplyColorToDropdown(_dropdown, targetColor);
                return;
            }

            if (_button != null && modifyButtonColorBlock)
            {
                ColorBlock cb = _button.colors;
                cb.normalColor = targetColor;

                cb.highlightedColor = (role == ColorRole.Correct || role == ColorRole.Incorrect)
                    ? Opaque(Color.Lerp(targetColor, Color.white, 0.25f))
                    : Opaque(AppColorManager.Instance.GetButtonShinyColor());

                cb.pressedColor = Opaque(AppColorManager.Instance.GetSecondaryColor());
                cb.selectedColor = targetColor;
                _button.colors = cb;
            }

            if (_image != null) _image.color = targetColor;
            if (_text != null) _text.color = targetColor;
        }
        catch (Exception ex)
        {
            // Si algo falla, te avisará con el nombre del objeto exacto en la consola
            Debug.LogError($"[UIColorElement] Error al colorear el objeto '{gameObject.name}': {ex.Message}");
        }
    }

    private Color GetColorFromRole(ColorRole targetRole)
    {
        return targetRole switch
        {
            ColorRole.Primary => AppColorManager.Instance.GetPrimaryColor(),
            ColorRole.Secondary => AppColorManager.Instance.GetSecondaryColor(),
            ColorRole.Light => AppColorManager.Instance.GetLightColor(),
            ColorRole.ButtonColor1 => AppColorManager.Instance.GetButtonColor1(),
            ColorRole.ButtonColor2 => AppColorManager.Instance.GetButtonColor2(),
            ColorRole.ButtonShiny => AppColorManager.Instance.GetButtonShinyColor(),
            ColorRole.Correct => Color.Lerp(AppColorManager.Instance.CorrectColor, Color.white, colorSaturation),
            ColorRole.Incorrect => Color.Lerp(AppColorManager.Instance.IncorrectColor, Color.white, colorSaturation),
            _ => Color.white
        };
    }

    private void ApplyColorToDropdown(TMP_Dropdown dropdown, Color baseColor)
    {
        if (_image != null) _image.color = baseColor;

        if (dropdown.template != null)
        {
            Image templateBg = dropdown.template.GetComponent<Image>();
            if (templateBg != null) templateBg.color = Opaque(AppColorManager.Instance.GetPrimaryColor());

            Toggle itemToggle = dropdown.template.GetComponentInChildren<Toggle>(true);
            if (itemToggle != null)
            {
                ColorBlock cb = itemToggle.colors;
                cb.normalColor = baseColor;
                cb.highlightedColor = Opaque(AppColorManager.Instance.GetButtonShinyColor());
                cb.pressedColor = Opaque(AppColorManager.Instance.GetSecondaryColor());
                cb.selectedColor = baseColor;
                itemToggle.colors = cb;

                TextMeshProUGUI itemText = itemToggle.GetComponentInChildren<TextMeshProUGUI>(true);
                if (itemText != null) itemText.color = Opaque(AppColorManager.Instance.GetLightColor());
            }
        }
    }
}