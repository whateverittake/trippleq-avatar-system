using System.Collections;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarScreenController : MonoBehaviour
    {
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
            while (!AvatarServiceLocator.IsReady)
                yield return null;

            var svc = AvatarServiceLocator.Service;

            svc.OnAvatarChanged += OnAvatarChanged;
            svc.OnInventoryChanged += Refresh;

            Refresh();
            OnAvatarChanged(svc.GetSelectedAvatarId().value);
        }

        void Refresh()
        {
            //grid.Bind(AvatarSystemBootstrap.Service);
        }

        void OnAvatarChanged(AvatarId id)
        {
            //preview.SetAvatar(id);
        }
    }
}
