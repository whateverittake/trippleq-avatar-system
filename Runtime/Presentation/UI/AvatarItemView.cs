using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrippleQ.AvatarSystem
{
    public sealed class AvatarItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon, frame;
        [SerializeField] private GameObject selectedRing;
        [SerializeField] GameObject _focusRing;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private TMP_Text _text; // optional

        private Action<AvatarDefinition> _onClick;

        public AvatarDefinition Def { get; private set; }

        public void Setup(AvatarDefinition def, Action<AvatarDefinition> onClick)
        {
            Def = def;
            _onClick = onClick;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => 
                {
                    _onClick?.Invoke(Def);
                    ShowFocus(true);
                });
            }

            if (_text != null)
                _text.text = string.IsNullOrEmpty(def.displayName) ? def.id : def.displayName;
        }

        public void Render(Sprite iconSprite, bool owned, bool selected)
        {
            if (icon != null) icon.sprite = iconSprite;

            if (selectedRing != null) selectedRing.SetActive(selected);

            bool locked = !owned;
            if (lockOverlay != null) lockOverlay.SetActive(locked);

            if(selected)
            {
                ShowFocus(true);
            }
            else
            {
                ShowFocus(false);
            }
        }

        public void ShowFocus(bool isShow)
        {
            _focusRing.SetActive(isShow);
        }

        public void UpDateFrame(Sprite frame)
        {
            this.frame.sprite= frame;
        }

        public void UpDateAvatar(Sprite avatarId)
        {
            icon.sprite = avatarId;
        }

        public void Refresh()
        {
            if (AvatarServiceLocator.Service == null)
            {
                Debug.LogWarning("[AvatarItemView] AvatarService is not ready.");
                return;
            }

            var info = AvatarServiceLocator.Service.GetAvatarInfo();
            var iconSprite = AvatarIconResolver.Get(info.selectedAvatarId);
            UpDateAvatar(iconSprite);
            var frameSprite = AvatarIconResolver.GetFrame(info.selectedFrameId);
            UpDateFrame(frameSprite);
        }
    }
}
