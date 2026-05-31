using UnityEngine;

public enum UnlockRequirementType
{
    None,               // Siempre desbloqueado
    TotalStars,         // Requiere un numero de estrellas
    StreakDays,          // Requiere dias de racha
    SQLLevelPassed,     // Requiere haber superado un nivel de SQL
    BiologiaLevelPassed // Requiere haber superado un nivel de Biologia
}

[CreateAssetMenu(fileName = "NewTitle", menuName = "Profile/Title Data", order = 1)]
public class ProfileTitleSO : ScriptableObject
{
    [Header("Configuracion Interna")]
    public string id;

    [Header("Lo que ve el jugador")]
    public string titleName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public Color titleColor = Color.white;

    [Header("Requisitos de Desbloqueo")]
    [Tooltip("Que hay que hacer para conseguirlo?")]
    public UnlockRequirementType requirementType;

    [Tooltip("El valor necesario. Ej: Si elegiste 'TotalStars' y pones 50, necesitas 50 estrellas. Si elegiste 'SQLLevelPassed' y pones 3, necesitas superar sql-3.")]
    public int requirementValue;
}