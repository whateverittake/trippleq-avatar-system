using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarWidgetController : MonoBehaviour
    {
        [SerializeField] AvatarWidgetView view;

        public void SetUpEvent()
        {
            if (AvatarServiceLocator.Service != null) AvatarServiceLocator.Service.OnAvatarChanged -= OnAvatarChanged;
            if (AvatarServiceLocator.Service != null) AvatarServiceLocator.Service.OnFrameChanged -= OnFrameChanged;
            AvatarServiceLocator.Service.OnAvatarChanged += OnAvatarChanged;
            AvatarServiceLocator.Service.OnFrameChanged += OnFrameChanged;
            Refresh();
        }

        void OnAvatarChanged(AvatarId id)
        {
            var sprite = AvatarIconResolver.Get(id.Value);
            view.SetIcon(sprite);
        }

        void OnFrameChanged(AvatarId id)
        {
            var sprite = AvatarIconResolver.GetFrame(id.Value);
            view.SetFrame(sprite);
        }

        private void Refresh()
        {
            //var currentAvatarId = AvatarServiceLocator.Service.GetCurrentAvatarId();
            //var sprite = AvatarIconResolver.Get(currentAvatarId.Value);
            //view.SetIcon(sprite);
        }
    }
}
