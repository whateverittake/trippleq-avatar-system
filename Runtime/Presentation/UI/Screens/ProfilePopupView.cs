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

        private Action _onSave;
        private Action _onClosePopup;
        private Action _onEditName;
        private Action<AvatarId> _onClickAvatar;
        private Action<AvatarId> _onClickFrame;

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

        public void Show()
        {
            gameObject.SetActive(true);
            _nameInputField.interactable = false;
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

        public void SetOwned(AvatarId id, bool owned)
        {

        }

        public void SetOwnedFrame(AvatarId id, bool owned)
        {

        }

        public void SetLockedFrame(AvatarId id, bool locked)
        {

        }

        //set avatar is locked or not
        public void SetLocked(AvatarId id, bool locked)
        {

        }

        #endregion
    }
}
