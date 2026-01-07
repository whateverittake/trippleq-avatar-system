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

            View.SetClose(Hide);

            // subscribe service events
            _svc.OnAvatarChanged += OnAvatarChanged;
            _svc.OnInventoryChanged += OnInventoryChanged;

            // initial render
            RenderAll();
        }

        protected override void OnUnbind()
        {
            // unsubscribe service events
            _svc.OnAvatarChanged -= OnAvatarChanged;
            _svc.OnInventoryChanged -= OnInventoryChanged;

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
                    _svc.TrySelect(id);
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

        private void RenderAll()
        {
            // list items
            View.SetItems(_db.avatars);

            // selected + preview
            var selectedRes = _svc.GetSelectedAvatarId();

            if (selectedRes.ok)
            {
                View.SetSelected(selectedRes.value);
                View.SetPreview(selectedRes.value);
            }

            // owned/locked state for all
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

        private void OnAvatarChanged(AvatarId id)
        {
            View.SetSelected(id);
            View.SetPreview(id);
        }

        private void OnInventoryChanged()
        {
            // simplest: rerender states
            RenderAll();
        }
    }
}
