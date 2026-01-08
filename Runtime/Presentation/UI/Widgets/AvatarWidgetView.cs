using UnityEngine;
using UnityEngine.UI;

namespace TrippleQ.AvatarSystem
{
    public class AvatarWidgetView : MonoBehaviour
    {
        [SerializeField] Image avatarIcon, frame;
        public void SetIcon(Sprite s) => avatarIcon.sprite = s;
        public void SetFrame(Sprite s) => frame.sprite = s;
    }
}
