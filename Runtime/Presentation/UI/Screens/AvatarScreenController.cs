using System.Collections;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    //“công tắc mở / đóng màn hình avatar + nơi inject UI framework vào Avatar package”.
    public class AvatarScreenController : MonoBehaviour
    {
        [SerializeField] GameObject _avatarPopup;
        private AvatarPopupPresenter _presenter;
        private IAvatarPopupView _view;

        void OnEnable()
        {
            StartCoroutine(BindWhenReady());
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

            if (_presenter == null)
            {
                _presenter = new AvatarPopupPresenter(AvatarServiceLocator.Service, svc.GetAvatarDatabaseSO());
            }

            GetComponent<AvatarWidgetController>().SetUpEvent();
        }

        public void OpenAvatarPopup()
        {
            GameObject popup = null; //tuy framework, game sẽ tạo popup từ hệ thống UI của nó
            _view = _avatarPopup.GetComponent<IAvatarPopupView>();

            _presenter.Bind(_view);
            _presenter.Show();
        }

        public void CloseAvatarPopup() 
        {
            if(_view == null) return;

            _presenter.Hide();
            _presenter.Unbind();

            _view = null;
        }

        private void OnDisable()
        {
            // cleanup lifecycle only
            CloseAvatarPopup();
        }

        public void OnAvatarWidgetClicked()
        {
            OpenAvatarPopup();
        }
    }
}
