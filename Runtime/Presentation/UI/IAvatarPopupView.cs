using System;
using System.Collections.Generic;
using TrippleQ.UiKit;

namespace TrippleQ.AvatarSystem
{
    public interface IAvatarPopupView : ITrippleQPopupView
    {
        // render list
        void SetItems(IReadOnlyList<AvatarDefinition> defs);

        void SetFrameItems(IReadOnlyList<AvatarDefinition> defs);

        // state updates
        //update selected state of avatar item
        void SetSelected(AvatarId id);
        void SetSelectedFrame(AvatarId id);
        //update owned state of avatar item
        void SetOwned(AvatarId id, bool owned);
        //update locked state of list avatar
        void SetLocked(AvatarId id, bool locked);
        //update owned state of avatar item
        void SetOwnedFrame(AvatarId id, bool owned);
        //update locked state of list avatar
        void SetLockedFrame(AvatarId id, bool locked);

        // click on a avatar callback
        void SetOnItemClicked(Action<AvatarId> onClick);
        void SetOnItemFrameClicked(Action<AvatarId> onClick);

        void SetOnEditNameClicked(Action onClick);

        void SetUserName(string userName);
        void SetNameEditing(bool editing);
        void ClearNameInput();
        void FocusNameInput();
        string GetNameInput();

        void SetAvatar(string avatarId);
        void SetFrame(string frameId);
        AvatarId GetSelectedAvatarId();
        AvatarId GetSelectedFrameId();
    }
}
