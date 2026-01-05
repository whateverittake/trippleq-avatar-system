using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public sealed class BasicAvatarUnlockProvider : IAvatarUnlockProvider
    {
        public bool CanUnlock(AvatarDefinition def, AvatarUserState userState, out string reason)
        {
            reason = null;
            if (def == null) { reason = "Missing definition."; return false; }

            switch (def.unlockType)
            {
                case AvatarUnlockType.Free:
                case AvatarUnlockType.Default:
                    return true;

                case AvatarUnlockType.RewardedAd:
                    return false;

                case AvatarUnlockType.SoftCurrency:
                    return false;

                case AvatarUnlockType.PlayerLevel:
                    return false;

                default:
                    reason = "Not unlockable";
                    return false;
            }
        }

        public bool TryUnlock(AvatarDefinition def, AvatarUserState userState, out string error)
        {
            error = null;
            switch (def.unlockType)
            {
                case AvatarUnlockType.Free:
                case AvatarUnlockType.Default:
                    return true;

                case AvatarUnlockType.SoftCurrency:
                    error = "Not enough coins";
                    return false;

                case AvatarUnlockType.RewardedAd:
                    error = "Ad failed";
                    return false;
            }

            error = "Unlock failed";
            return false;
        }
    }

    public sealed class NullAvatarLogger : IAvatarLogger
    {
        public void Info(string msg) { }
        public void Warn(string msg) { }
        public void Error(string msg) { }
    }
}
