using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarService
    {
        public event Action OnInitialized;               // fired when initialized
        public event Action<AvatarId> OnAvatarChanged;          // fired when selected changes
        public event Action OnInventoryChanged;                 // fired when owned list changes

        private AvatarDatabaseSO _db;
        private IAvatarStorage _storage;
        private IAvatarUnlockProvider _unlockProvider;
        private IAvatarLogger _log;

        private AvatarUserState _state;
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public AvatarResult Initialize(
            AvatarDatabaseSO database,
            IAvatarStorage storage,
            IAvatarUnlockProvider unlockProvider,
            IAvatarLogger logger = null)
        {
            _db = database;
            _storage = storage;
            _unlockProvider = unlockProvider;
            _log = logger ?? new NullAvatarLogger();

            if (_db == null) return AvatarResult.Fail(AvatarError.NotInitialized, "Database is null.");
            if (_storage == null) return AvatarResult.Fail(AvatarError.NotInitialized, "Storage is null.");
            if (_unlockProvider == null) return AvatarResult.Fail(AvatarError.NotInitialized, "UnlockProvider is null.");

            _db.BuildIndex();

            // Load
            if (!_storage.TryLoad(out _state))
            {
                _log.Warn("AvatarService: storage load failed; using fresh state.");
                _state = new AvatarUserState();
            }

            // Ensure default owned
            var def = _db.GetDefaultOrFirst();
            if (def != null)
            {
                var defId = def.AvatarId;

                // Default avatars should be owned by default (common casual pattern).
                if (!_state.Owns(defId))
                    _state.AddOwned(defId);

                // Ensure selected exists
                if (string.IsNullOrWhiteSpace(_state.selectedAvatarId))
                    _state.selectedAvatarId = defId.Value;
            }

            // Repair selection if invalid
            var currentId = new AvatarId(_state.selectedAvatarId);
            if (!currentId.IsValid || !_db.TryGet(currentId, out _))
            {
                var fallback = _db.GetDefaultOrFirst();
                _state.selectedAvatarId = fallback != null ? fallback.id : string.Empty;
            }

            // Save back repaired state
            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: storage save failed during Initialize.");
            }

            _initialized = true;
            OnInitialized?.Invoke();

            return AvatarResult.Ok();
        }

        public AvatarDatabaseSO GetAvatarDatabaseSO()
        {
            return _db;
        }

        public AvatarResult<AvatarUserState> GetUserStateSnapshot()
        {
            if (!_initialized) return AvatarResult<AvatarUserState>.Fail(AvatarError.NotInitialized, "Not initialized.");
            // shallow copy for safety
            var snap = new AvatarUserState
            {
                selectedAvatarId = _state.selectedAvatarId,
                ownedAvatarIds = new List<string>(_state.ownedAvatarIds ?? new List<string>())
            };
            return AvatarResult<AvatarUserState>.Ok(snap);
        }

        public AvatarResult<AvatarId> GetSelectedAvatarId()
        {
            if (!_initialized) return AvatarResult<AvatarId>.Fail(AvatarError.NotInitialized, "Not initialized.");
            var id = new AvatarId(_state.selectedAvatarId);
            if (!id.IsValid) return AvatarResult<AvatarId>.Fail(AvatarError.InvalidId, "Selected avatar id invalid.");
            return AvatarResult<AvatarId>.Ok(id);
        }

        public AvatarResult<AvatarDefinition> GetSelectedAvatarDefinition()
        {
            if (!_initialized) return AvatarResult<AvatarDefinition>.Fail(AvatarError.NotInitialized, "Not initialized.");

            var id = new AvatarId(_state.selectedAvatarId);
            if (!id.IsValid) return AvatarResult<AvatarDefinition>.Fail(AvatarError.InvalidId, "Selected avatar id invalid.");

            if (!_db.TryGet(id, out var def))
                return AvatarResult<AvatarDefinition>.Fail(AvatarError.NotFoundInDatabase, $"Avatar '{id.Value}' not in database.");

            return AvatarResult<AvatarDefinition>.Ok(def);
        }

        public AvatarResult<AvatarOwnershipState> GetAvatarState(AvatarId id)
        {
            if (!_initialized) return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGet(id, out var def))
                return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.NotFoundInDatabase, "Not in database.", AvatarOwnershipState.Unknown);

            var selected = string.Equals(_state.selectedAvatarId, id.Value, StringComparison.Ordinal);
            var owned = _state.Owns(id);

            if (selected && owned) return AvatarResult<AvatarOwnershipState>.Ok(AvatarOwnershipState.Selected);
            if (owned) return AvatarResult<AvatarOwnershipState>.Ok(AvatarOwnershipState.Owned);

            var canUnlock = _unlockProvider.CanUnlock(def, _state, out _);
            return AvatarResult<AvatarOwnershipState>.Ok(canUnlock ? AvatarOwnershipState.Unlockable : AvatarOwnershipState.Locked);
        }

        public AvatarResult TrySelect(AvatarId id)
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGet(id, out _))
                return AvatarResult.Fail(AvatarError.NotFoundInDatabase, $"Avatar '{id.Value}' not in database.");

            if (!_state.Owns(id))
                return AvatarResult.Fail(AvatarError.NotOwned, "Avatar not owned.");

            if (string.Equals(_state.selectedAvatarId, id.Value, StringComparison.Ordinal))
                return AvatarResult.Ok("Already selected.");

            _state.selectedAvatarId = id.Value;

            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: save failed on select.");
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save selection.");
            }

            OnAvatarChanged?.Invoke(id);
            return AvatarResult.Ok();
        }

        public AvatarResult TryUnlock(AvatarId id)
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGet(id, out var def))
                return AvatarResult.Fail(AvatarError.NotFoundInDatabase, $"Avatar '{id.Value}' not in database.");

            if (_state.Owns(id))
                return AvatarResult.Ok("Already owned.");

            if (!_unlockProvider.CanUnlock(def, _state, out var reason))
                return AvatarResult.Fail(AvatarError.NotUnlockable, reason ?? "Not unlockable.");

            if (!_unlockProvider.TryUnlock(def, _state, out var err))
                return AvatarResult.Fail(AvatarError.UnlockProviderFailure, err ?? "Unlock failed.");

            _state.AddOwned(id);

            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: save failed on unlock.");
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save unlock.");
            }

            OnInventoryChanged?.Invoke();
            return AvatarResult.Ok();
        }

        // Convenience: unlock then select (common UX)
        public AvatarResult TryUnlockAndSelect(AvatarId id)
        {
            var unlock = TryUnlock(id);
            if (!unlock.ok && unlock.error != AvatarError.None)
            {
                // If already owned, proceed to select
                if (unlock.error != AvatarError.None && unlock.error != AvatarError.NotUnlockable && unlock.error != AvatarError.UnlockProviderFailure)
                {
                    // For cases like StorageFailure, stop
                    if (unlock.error == AvatarError.StorageFailure) return unlock;
                }
            }

            // If not owned and unlock failed -> stop
            if (!_state.Owns(id)) return unlock.ok ? AvatarResult.Fail(AvatarError.NotOwned, "Unlock did not grant ownership.") : unlock;

            return TrySelect(id);
        }

        // Admin / debug: grant ownership directly (e.g., migration, cheat, support tool)
        public AvatarResult GrantOwned(AvatarId id, bool autoSelectIfNone = true)
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGet(id, out _))
                return AvatarResult.Fail(AvatarError.NotFoundInDatabase, $"Avatar '{id.Value}' not in database.");

            if (!_state.Owns(id))
            {
                _state.AddOwned(id);
                OnInventoryChanged?.Invoke();
            }

            if (autoSelectIfNone && string.IsNullOrWhiteSpace(_state.selectedAvatarId))
            {
                _state.selectedAvatarId = id.Value;
                OnAvatarChanged?.Invoke(id);
            }

            if (!_storage.TrySave(_state))
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save grant.");

            return AvatarResult.Ok();
        }
    }
}
