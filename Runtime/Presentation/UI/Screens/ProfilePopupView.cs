using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class ProfilePopupView : MonoBehaviour, IAvatarPopupView
    {
        [SerializeField] TMP_InputField _nameInputField;
        [SerializeField] AvatarItemView _userProfile;

        [SerializeField] AvatarItemView[] _avatarItems;
        [SerializeField] AvatarItemView[] _frameItems;

        private Action _onSave;
        private Action _onClosePopup;
        private Action _onEditName;
        private Action<AvatarId> _onClickAvatar;
        private Action<AvatarId> _onClickFrame;

        private AvatarId _focusAvatarId;
        private AvatarId _focusFrameId;

        public bool IsVisible => gameObject.activeSelf;

        public void OnSaveBtnClick()
        {
            _onSave?.Invoke();
        }

        public void OnClosePopup()
        {
            _onClosePopup?.Invoke();
        }

        public void OnEditNameBtnClick()
        {
            _onEditName?.Invoke();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _nameInputField.interactable = false;

        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        //set close action - quit popup
        public void SetClose(Action onClick)
        {
            _onClosePopup = onClick;
        }

        //data for avatar items to fill grid view
        public void SetItems(IReadOnlyList<AvatarDefinition> defs)
        {
            bool owned = false;
            bool selected = false;

            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                var itemView = _avatarItems[i];
                itemView.Setup(def, (d) => 
                {
                    RefreshFocusAvatarItem();
                    _onClickAvatar?.Invoke(d.AvatarId);
                    _focusAvatarId= d.AvatarId;
                    SetAvatar(d.id);
                });

                owned= AvatarServiceLocator.Service.GetUserStateSnapshot().value.Owns(def.AvatarId);
                selected= AvatarServiceLocator.Service.GetSelectedAvatarId().value == def.AvatarId;

                itemView.Render(AvatarIconResolver.Get(def.id), owned, selected);
            }
        }

        public void SetFrameItems(IReadOnlyList<AvatarDefinition> defs)
        {
            bool owned = false;
            bool selected = false;

            for (int i = 0; i < defs.Count; i++)
            {
                var def = defs[i];
                var itemView = _frameItems[i];
                itemView.Setup(def, (d) => 
                {
                    RefreshFocusFrameItem();
                    _onClickFrame?.Invoke(d.AvatarId);
                    _focusFrameId= d.AvatarId;
                    SetFrame(d.id);
                });

                owned = AvatarServiceLocator.Service.GetUserStateSnapshot().value.OwnFrame(def.AvatarId);
                selected = AvatarServiceLocator.Service.GetSelectedFrameId().value == def.AvatarId;

                itemView.Render(AvatarIconResolver.GetFrame(def.id), owned, selected);
            }
        }

        public void SetMessage(string message)
        {
           
        }

        //set action when click on avatar item
        public void SetOnItemClicked(Action<AvatarId> onClick)
        {
            _onClickAvatar = onClick;
        }

        public void SetOnItemFrameClicked(Action<AvatarId> onClick)
        {
            _onClickFrame = onClick;
        }

        public void SetPrimary(string label, Action onClick)
        {
            _onSave = onClick;
        }

        public void SetSecondary(string label, Action onClick)
        {
            
        }

        public void SetSelected(AvatarId id)
        {
            SetAvatar(id.Value);
        }

        public void SetSelectedFrame(AvatarId id)
        {
            SetFrame(id.Value);
        }

        public void SetTitle(string title)
        {
            
        }

        public void OnUnfocusEditName()
        {
            _nameInputField.interactable = false;
        }

        #region EDIT NAME
        public void SetOnEditNameClicked(Action onClick)
        {
            _onEditName = onClick;
        }

        public void SetUserName(string userName)
        {
            _nameInputField.text = userName;
        }

        public void SetNameEditing(bool editing)
        {
            _nameInputField.interactable = editing;
        }

        public void ClearNameInput()
        {
            _nameInputField.text = string.Empty;
        }

        public void FocusNameInput()
        {
            _nameInputField.Select();
            _nameInputField.ActivateInputField();
        }

        public string GetNameInput()
        {
            return _nameInputField.text;
        }
        #endregion

        #region USER
        public void SetFrame(string frameId)
        {
            //update frame image
            Sprite sprite = AvatarIconResolver.GetFrame(frameId);
            _userProfile.UpDateFrame(sprite);
        }

        public void SetAvatar(string avatarId)
        {
            //update avatar image
            Sprite sprite = AvatarIconResolver.Get(avatarId);
            _userProfile.UpDateAvatar(sprite);
        }
        #endregion

        #region GRID VIEW ITEM STATE UPDATES
        private void RefreshFocusAvatarItem()
        {
            foreach (var item in _avatarItems)
            {
                item.ShowFocus(false);
            }
        }

        private void RefreshFocusFrameItem()
        {
            foreach (var item in _frameItems)
            {
                item.ShowFocus(false);
            }
        }

        public void SetOwned(AvatarId id, bool owned)
        {

        }
        //set avatar is locked or not
        public void SetLocked(AvatarId id, bool locked)
        {

        }

        //frame owned state
        public void SetOwnedFrame(AvatarId id, bool owned)
        {

        }

        public void SetLockedFrame(AvatarId id, bool locked)
        {

        }

        public AvatarId GetSelectedAvatarId()
        {
            return _focusAvatarId;
        }

        public AvatarId GetSelectedFrameId()
        {
            return _focusFrameId;
        }
        #endregion
    }
}
