using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    [CreateAssetMenu(menuName = "TrippleQ/Avatar System/Icon Resolver", fileName = "AvatarIconResolver")]
    public class AvatarIconResolverSO : ScriptableObject
    {
        [Tooltip("Folder path under Assets, e.g. Assets/GameA/Avatars/Icons")]
        public string iconsFolder = "Assets/GameA/Avatars/Icons";

        [Tooltip("File extensions to try")]
        public string[] exts = new[] { "png", "jpg", "jpeg" };
    }
}
