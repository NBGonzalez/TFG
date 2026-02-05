using UnityEngine;

public enum UnlockRequirementType
{
    None,           // Siempre desbloqueado
    TotalStars,     // Requiere un número de estrellas
    StreakDays,     // Requiere días de racha
    // LevelSpecific // (Podríamos añadirlo en el futuro)
}

[CreateAssetMenu(fileName = "NewTitle", menuName = "Profile/Title Data", order = 1)]
public class ProfileTitleSO : ScriptableObject
{
    [Header("Configuración Interna")]
    public string id;

    [Header("Lo que ve el jugador")]
    public string titleName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public Color titleColor = Color.white;

    [Header("Requisitos de Desbloqueo")]
    [Tooltip("¿Qué hay que hacer para conseguirlo?")]
    public UnlockRequirementType requirementType;

    [Tooltip("El valor necesario. Ej: Si elegiste 'TotalStars' y pones 50, necesitas 50 estrellas.")]
    public int requirementValue;
}