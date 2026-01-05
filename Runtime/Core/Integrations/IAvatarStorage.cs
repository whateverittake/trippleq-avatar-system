using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public interface IAvatarStorage
    {
        // Return false if read fails; service will fallback to default state.
        bool TryLoad(out AvatarUserState state);

        // Return false if write fails.
        bool TrySave(AvatarUserState state);
    }

    // Determines whether an avatar is unlockable (and performs unlock action if requested).
    // This is where you integrate coins/ads/level/event.
    public interface IAvatarUnlockProvider
    {
        // Evaluate if unlock is possible right now.
        bool CanUnlock(AvatarDefinition def, AvatarUserState userState, out string reason);

        // Execute unlock transaction (spend currency, show ad, etc.)
        // If success, return true. If you show ad async, you can still keep core sync by
        // offering your own wrapper in UI layer; OR extend this interface to async later.
        bool TryUnlock(AvatarDefinition def, AvatarUserState userState, out string error);
    }

    public interface IAvatarLogger
    {
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg);
    }
}
