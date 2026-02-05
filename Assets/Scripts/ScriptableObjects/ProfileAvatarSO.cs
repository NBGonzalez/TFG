using UnityEngine;

[CreateAssetMenu(fileName = "NewAvatar", menuName = "Profile/Avatar Data", order = 2)]
public class ProfileAvatarSO : ScriptableObject
{
    public string id;           // Ej: "avatar_mago"
    public Sprite avatarImage;  // La foto
    public bool isFree = true;  // ¿Es gratis o hay que desbloquearlo?

    // Si no es gratis, aquí podríamos poner un requisito (ej: ID de un logro necesario)
    public string requiredAchievementId;
}