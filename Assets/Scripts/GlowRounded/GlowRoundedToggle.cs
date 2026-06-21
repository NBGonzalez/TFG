using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class GlowRoundedToggle : MonoBehaviour
{
    [SerializeField] bool isRounded = true;

    Graphic graphic;
    Material instance;
    static readonly int RoundedId = Shader.PropertyToID("_IsRounded");

    void Awake()
    {
        graphic = GetComponent<Graphic>();
        // graphic.material crea y devuelve una instancia propia (no toca el asset)
        instance = graphic.material;
        Apply();
    }

    void Apply()
    {
        if (instance) instance.SetFloat(RoundedId, isRounded ? 1f : 0f);
    }

    // Permite que AppColorManager (u otro código) cambie la forma en runtime
    public void SetRounded(bool value)
    {
        isRounded = value;
        Apply();
    }
}