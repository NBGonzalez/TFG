// NotificationData.cs
using UnityEngine;

/// <summary>
/// Datos genericos de una notificacion.
/// Cualquier sistema puede crear un NotificationData y encolarlo.
/// </summary>
public class NotificationData
{
    public Sprite icon;             // Imagen a mostrar
    public string defaultText;      // Texto superior (ej: "¡Has desbloqueado!")
    public string titleText;        // Texto inferior (ej: nombre del titulo o recompensa)
    public string targetState;      // Estado al que navegar si el jugador pulsa (ej: "Profile", null = no navegar)

    public NotificationData(Sprite icon, string defaultText, string titleText, string targetState = null)
    {
        this.icon = icon;
        this.defaultText = defaultText;
        this.titleText = titleText;
        this.targetState = targetState;
    }
}