using System.Collections;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarScreenController : MonoBehaviour
    {
        [SerializeField] Transform _popupRoot; // Canvas / UI root

        [SerializeField] GameObject _avatar;

        public IPopupPresenter PopupPresenter { get; set; }

        //[SerializeField] AvatarGridView grid;
        //[SerializeField] AvatarPreviewView preview;

        void OnEnable()
        {
            StartCoroutine(BindWhenReady());
        }

        void OnDisable()
        {
            if (!AvatarServiceLocator.IsReady) return;

            var svc = AvatarServiceLocator.Service;
            svc.OnAvatarChanged -= OnAvatarChanged;
            svc.OnInventoryChanged -= Refresh;
        }

        IEnumerator BindWhenReady()
        {
            const float timeout = 5f;
            float t = 0f;

            while (!AvatarServiceLocator.IsReady)
            {
                t += Time.unscaledDeltaTime;
                if (t >= timeout)
                {
                    Debug.LogError(
                        "[AvatarScreenController] Timeout waiting for AvatarService. " +
                        "Did you forget AvatarSystemBootstrap?"
                    );
                    yield break;
                }
                yield return null;
            }

            var svc = AvatarServiceLocator.Service;

            svc.OnAvatarChanged += OnAvatarChanged;
            svc.OnInventoryChanged += Refresh;

            if (PopupPresenter == null)
            {
                if (_popupRoot == null)
                    _popupRoot = transform; // fallback

                PopupPresenter = new DefaultPopupPresenter(_popupRoot);
            }

            Refresh();
            OnAvatarChanged(svc.GetSelectedAvatarId().value);
        }

        private void Refresh()
        {
            //grid.Bind(AvatarSystemBootstrap.Service);

            //iconImage.sprite = AvatarIconResolver.Get(avatar01);
        }

        private void OnAvatarChanged(AvatarId id)
        {
            //preview.SetAvatar(id);
        }

        public void ShowAvatarPopup()
        {
            PopupPresenter.Show(_avatar);
        }

        public void CloseAvatarPopup()
        {
            PopupPresenter.Close(_avatar);
        }
    }
}
