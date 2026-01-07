using UnityEngine;
using UnityEngine.UI;

namespace TrippleQ.AvatarSystem
{
    public class AvatarWidgetView : MonoBehaviour
    {
        [SerializeField] Image avatarIcon;
        public void SetIcon(Sprite s) => avatarIcon.sprite = s;
    }
}
