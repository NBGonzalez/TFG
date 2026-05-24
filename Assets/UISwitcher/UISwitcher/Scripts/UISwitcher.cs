using UnityEngine;
using UnityEngine.UI;

namespace UISwitcher
{
    public class UISwitcher : UINullableToggle
    {
        private readonly Vector2 _min = new(0, 0.5f);
        private readonly Vector2 _max = new(1, 0.5f);
        private readonly Vector2 _middle = new(0.5f, 0.5f);

        [SerializeField] private Graphic backgroundGraphic;
        [SerializeField] private Color nullColor = Color.gray;
        [SerializeField] private RectTransform tipRect;

        private Color DynamicOnColor => AppColorManager.Instance != null ? AppColorManager.Instance.CorrectColor : Color.green;
        private Color DynamicOffColor => AppColorManager.Instance != null ? AppColorManager.Instance.IncorrectColor : Color.red;

        private Color backgroundColor
        {
            set
            {
                if (backgroundGraphic == null) return;
                backgroundGraphic.color = value;
            }
        }

        // ========================================================
        // ¡EL TRUCO PARA EL DESPERTAR! 🌟
        // ========================================================
        private void OnEnable()
        {
            // Forzamos al Switcher a repintarse con su estado actual nada más abrirse el panel.
            //
            // NOTA DE COMPILACIÓN: Al ser un 'NullableToggle', la propiedad que guarda si está
            // True, False o Null suele llamarse 'Value' o 'isOn'. 
            //
            // 1. Si te da ERROR diciendo que 'Value' no existe -> Cámbialo por 'isOn'.
            // 2. Si te da un WARNING diciendo que oculta un método heredado -> Cambia este método a:
            //    protected override void OnEnable() { base.OnEnable(); OnChanged(Value); }

            OnChanged(isOn);
        }

        protected override void OnChanged(bool? obj)
        {
            if (obj.HasValue)
            {
                if (obj.Value)
                    SetOn();
                else
                    SetOff();
            }
            else
            {
                SetNull();
            }
        }

        private void SetOn()
        {
            SetAnchors(_max);
            backgroundColor = DynamicOnColor;
        }

        private void SetOff()
        {
            SetAnchors(_min);
            backgroundColor = DynamicOffColor;
        }

        private void SetNull()
        {
            SetAnchors(_middle);
            backgroundColor = nullColor;
        }

        private void SetAnchors(Vector2 anchor)
        {
            tipRect.anchorMin = anchor;
            tipRect.anchorMax = anchor;
            tipRect.pivot = anchor;
        }
    }
}