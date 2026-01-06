using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class DefaultPopupPresenter : IPopupPresenter
    {
        private readonly Transform _root;

        public DefaultPopupPresenter(Transform root)
        {
            _root = root;
        }

        public void Close(GameObject popupInstance)
        {
            if (popupInstance != null)
                popupInstance.SetActive(false);
        }

        public GameObject Show(GameObject popupPrefab)
        {
            if (popupPrefab == null)
            {
                Debug.LogError("[DefaultPopupPresenter] Popup prefab is null");
                return null;
            }
            popupPrefab.SetActive(true);

            return popupPrefab;
        } 
    }
}
