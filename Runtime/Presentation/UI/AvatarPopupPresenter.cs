using TrippleQ.UiKit;

namespace TrippleQ.AvatarSystem
{
    public sealed class AvatarPopupPresenter : BasePopupPresenter<IAvatarPopupView>
    {
        private readonly AvatarService _svc;
        private readonly AvatarDatabaseSO _db;

        public AvatarPopupPresenter(AvatarService svc, AvatarDatabaseSO db)
        {
            _svc = svc;
            _db = db;
        }

        protected override void OnBind()
        {
            View.SetOnItemClicked(OnItemClicked);
            View.SetOnItemFrameClicked(OnItemFrameClicked);

            View.SetClose(() => 
            {
                Hide();
            });

            View.SetPrimary(string.Empty, () => 
            {
                var newName = View.GetNameInput();
                if (!string.IsNullOrWhiteSpace(newName))
                    _svc.TryUpdateUserName(newName);

                var selectedAvatarId = View.GetSelectedAvatarId();
                if(selectedAvatarId != null)
                {
                    _svc.TrySelect((AvatarId)selectedAvatarId);
                }

                var selectedFrameId = View.GetSelectedFrameId();
                if(selectedFrameId != null)
                {
                    _svc.TrySelectFrame((AvatarId)selectedFrameId);
                }

                AvatarResult result =  _svc.SaveProfile();

                if (result.ok)
                {

                }

                View.SetNameEditing(false);
                Hide();
            });

            View.SetOnEditNameClicked(() => 
            {
                //enable TMP_inputfield for edit name
                View.SetNameEditing(true);
                View.ClearNameInput();
                //View.FocusNameInput();
            });

            var snap = _svc.GetUserStateSnapshot();

            View.SetUserName(snap.value.userName);
            View.SetFrame(snap.value.selectedFrameId);
            View.SetAvatar(snap.value.selectedAvatarId);

            // subscribe service events
            _svc.OnAvatarChanged += OnAvatarChanged;
            _svc.OnInventoryChanged += OnInventoryChanged;
            _svc.OnUserNameChanged += View.SetUserName;
            _svc.OnFrameChanged += OnFrameChanged;

            // initial render
            RenderAll();
        }

        protected override void OnUnbind()
        {
            // unsubscribe service events
            _svc.OnAvatarChanged -= OnAvatarChanged;
            _svc.OnInventoryChanged -= OnInventoryChanged;
            _svc.OnUserNameChanged -= View.SetUserName;
            _svc.OnFrameChanged -= OnFrameChanged;

            View.SetOnItemClicked(null);
        }

        private void OnItemClicked(AvatarId id)
        {
            var stateRes = _svc.GetAvatarState(id);
            if (!stateRes.ok) return;

            switch (stateRes.value)
            {
                case AvatarOwnershipState.Owned:
                case AvatarOwnershipState.Selected:
                    //_svc.TrySelect(id);
                    break;

                case AvatarOwnershipState.Unlockable:
                    _svc.TryUnlockAndSelect(id);
                    break;

                default:
                    // Locked: tuỳ game, có thể phát event ra ngoài để show IAP/ads
                    // (Option 2: Presenter raise RequestUnlockUI(id))
                    break;
            }
        }

        private void OnItemFrameClicked(AvatarId id)
        {
            var stateRes = _svc.GetFrameState(id);
            if (!stateRes.ok) return;
            switch (stateRes.value)
            {
                case AvatarOwnershipState.Owned:
                case AvatarOwnershipState.Selected:
                    //_svc.TrySelectFrame(id);
                    break;
                case AvatarOwnershipState.Unlockable:
                    _svc.TryUnlockAndSelectFrame(id);
                    break;
                default:
                    // Locked: tuỳ game, có thể phát event ra ngoài để show IAP/ads
                    // (Option 2: Presenter raise RequestUnlockUI(id))
                    break;
            }
        }

        private void RenderAll()
        {
            // list items, get data from DB and update grid view
            View.SetItems(_db.avatars);
            View.SetFrameItems(_db.frames);

            // selected + preview
            var selectedRes = _svc.GetSelectedAvatarId();

            if (selectedRes.ok)
            {
                View.SetSelected(selectedRes.value);
            }

            var selectedFrameRes = _svc.GetSelectedFrameId();
            if (selectedFrameRes.ok)
            {
                View.SetSelectedFrame(selectedFrameRes.value);
            }

            // owned/locked state for all
            RefreshAvatarItemsOnGridState();
            RefreshFrameItemsOnGridState();
        }

        private void RefreshFrameItemsOnGridState()
        {
            foreach (var def in _db.frames)
            {
                if (def == null) continue;
                var id = def.AvatarId;

                var st = _svc.GetFrameState(id);
                if (!st.ok) continue;

                var owned = st.value == AvatarOwnershipState.Owned || st.value == AvatarOwnershipState.Selected;
                var locked = st.value == AvatarOwnershipState.Locked;

                View.SetOwnedFrame(id, owned);
                View.SetLockedFrame(id, locked);
            }
        }

        private void RefreshAvatarItemsOnGridState()
        {
            foreach (var def in _db.avatars)
            {
                if (def == null) continue;
                var id = def.AvatarId;

                var st = _svc.GetAvatarState(id);
                if (!st.ok) continue;

                var owned = st.value == AvatarOwnershipState.Owned || st.value == AvatarOwnershipState.Selected;
                var locked = st.value == AvatarOwnershipState.Locked;

                View.SetOwned(id, owned);
                View.SetLocked(id, locked);
            }
        }

        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            RenderAll();
        }

        private void OnAvatarChanged(AvatarId id)
        {
            View.SetSelected(id);
        }

        private void OnInventoryChanged()
        {
            // simplest: rerender states
            RenderAll();
        }

        private void OnFrameChanged(AvatarId id)
        {
            View.SetSelectedFrame(id);
        }
    }
}
