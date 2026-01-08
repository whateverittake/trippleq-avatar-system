using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public enum AvatarUnlockType
    {
        Default = 0,
        Free = 1,        // always unlockable for free
        SoftCurrency = 2,
        HardCurrency = 3,
        RewardedAd = 4,
        PlayerLevel = 5,
        Event = 6,       // gated by an external flag
        IAP = 7          // optional, implement in unlock provider
    }

    public enum AvatarOwnershipState
    {
        Unknown = 0,
        Locked = 1,
        Unlockable = 2,
        Owned = 3,
        Selected = 4
    }

    [Serializable]
    public struct AvatarId : IEquatable<AvatarId>
    {
        [SerializeField] private string _value;
        public string Value => _value;

        public AvatarId(string value) => _value = value ?? string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public override string ToString() => _value ?? string.Empty;
        public bool Equals(AvatarId other) => string.Equals(_value, other._value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is AvatarId other && Equals(other);
        public override int GetHashCode() => (_value ?? string.Empty).GetHashCode();
        public static bool operator ==(AvatarId a, AvatarId b) => a.Equals(b);
        public static bool operator !=(AvatarId a, AvatarId b) => !a.Equals(b);
    }

    // NOTE:
    // AvatarDefinition is reused for both Avatars and Frames.
    // Avatars affect player identity.
    // Frames are cosmetic only and must use frame APIs.
    [Serializable]
    public sealed class AvatarDefinition
    {
        [Tooltip("Unique string id - iconkey for load icon")]
        public string id;

        [Tooltip("Display name (optional, can be localized outside)")]
        public string displayName;

        [Tooltip("Unlock rule type")]
        public AvatarUnlockType unlockType = AvatarUnlockType.Default;

        [Tooltip("Unlock value meaning depends on unlockType (e.g., cost, required level, ad count, etc.)")]
        public int unlockValue = 0;

        [Tooltip("Optional tag for gating or grouping (e.g., 'halloween2026')")]
        public string tag;

        [Tooltip("Is this the default fallback avatar if no selection exists?")]
        public bool isDefault;

        public AvatarId AvatarId => new AvatarId(id);
    }

    [Serializable]
    public sealed class AvatarUserState
    {
        public string userName;
        public string selectedFrameId;
        public string selectedAvatarId;

        public List<string> ownedAvatarIds = new List<string>();
        public List<string> ownedFrameIds = new List<string>();

        public bool Owns(AvatarId id) => ownedAvatarIds != null && ownedAvatarIds.Contains(id.Value);
        public bool OwnFrame(AvatarId id) => ownedFrameIds != null && ownedFrameIds.Contains(id.Value);
        public void AddOwned(AvatarId id)
        {
            if (!id.IsValid) return;
            ownedAvatarIds ??= new List<string>();
            if (!ownedAvatarIds.Contains(id.Value)) ownedAvatarIds.Add(id.Value);
        }

        public void AddOwnedFrame(AvatarId id)
        {
            if(!id.IsValid) return; 
            ownedFrameIds ??= new List<string>();
            if (!ownedFrameIds.Contains(id.Value)) ownedFrameIds.Add(id.Value);
        }

        public void RemoveOwned(AvatarId id)
        {
            if (!id.IsValid) return;
            ownedAvatarIds?.Remove(id.Value);
        }
    }

    // Generic result types (avoid exceptions as flow control)
    public enum AvatarError
    {
        None = 0,
        NotInitialized,
        InvalidId,
        NotFoundInDatabase,
        NotOwned,
        NotUnlockable,
        StorageFailure,
        UnlockProviderFailure
    }

    public readonly struct AvatarResult
    {
        public readonly bool ok;
        public readonly AvatarError error;
        public readonly string message;

        private AvatarResult(bool ok, AvatarError error, string message)
        {
            this.ok = ok;
            this.error = error;
            this.message = message;
        }

        public static AvatarResult Ok(string message = null) => new AvatarResult(true, AvatarError.None, message);
        public static AvatarResult Fail(AvatarError error, string message = null) => new AvatarResult(false, error, message);
    }

    public readonly struct AvatarResult<T>
    {
        public readonly bool ok;
        public readonly AvatarError error;
        public readonly string message;
        public readonly T value;

        private AvatarResult(bool ok, AvatarError error, string message, T value)
        {
            this.ok = ok;
            this.error = error;
            this.message = message;
            this.value = value;
        }

        public static AvatarResult<T> Ok(T value, string message = null) => new AvatarResult<T>(true, AvatarError.None, message, value);
        public static AvatarResult<T> Fail(AvatarError error, string message = null, T value = default) => new AvatarResult<T>(false, error, message, value);
    }
}
