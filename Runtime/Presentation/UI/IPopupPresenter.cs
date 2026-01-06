using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public interface IPopupPresenter
    {
        /// <summary>
        /// Show popup prefab and return instance.
        /// Framework-specific implementation decides HOW.
        /// </summary>
        GameObject Show(GameObject popupPrefab);

        /// <summary>
        /// Close/destroy popup instance.
        /// </summary>
        void Close(GameObject popupInstance);
    }
}
