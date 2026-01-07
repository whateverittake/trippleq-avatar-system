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
        [SerializeField] private Image icon;
        [SerializeField] private GameObject selectedRing;
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
                button.onClick.AddListener(() => _onClick?.Invoke(Def));
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

            // nếu muốn: icon mờ khi locked
            //if (icon != null)
            //    icon.color = locked ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
        }
    }
}
