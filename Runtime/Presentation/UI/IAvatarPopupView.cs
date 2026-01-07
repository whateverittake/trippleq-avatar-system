using System;
using System.Collections.Generic;
using TrippleQ.UiKit;

namespace TrippleQ.AvatarSystem
{
    public interface IAvatarPopupView : ITrippleQPopupView
    {
        // render list
        void SetItems(IReadOnlyList<AvatarDefinition> defs);

        // state updates
        void SetSelected(AvatarId id);
        void SetOwned(AvatarId id, bool owned);
        void SetLocked(AvatarId id, bool locked);

        // click callback
        void SetOnItemClicked(Action<AvatarId> onClick);

        // preview (tuỳ UX)
        void SetPreview(AvatarId id);
    }
}
