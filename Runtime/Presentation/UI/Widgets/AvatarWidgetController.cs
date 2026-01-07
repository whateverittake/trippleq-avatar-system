using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarWidgetController : MonoBehaviour
    {
        [SerializeField] AvatarWidgetView view;

        void OnEnable()
        {
            AvatarServiceLocator.Service.OnAvatarChanged += OnAvatarChanged;
            Refresh();
        }

        void OnDisable()
        {
            AvatarServiceLocator.Service.OnAvatarChanged -= OnAvatarChanged;
        }

        void OnAvatarChanged(AvatarId id)
        {
            var sprite = AvatarIconResolver.Get(id.Value);
            view.SetIcon(sprite);
        }

        private void Refresh()
        {
            //var currentAvatarId = AvatarServiceLocator.Service.GetCurrentAvatarId();
            //var sprite = AvatarIconResolver.Get(currentAvatarId.Value);
            //view.SetIcon(sprite);
        }
    }
}
