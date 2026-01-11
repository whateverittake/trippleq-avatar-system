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
        public event Action<string> OnUserNameChanged;
        public event Action<AvatarId> OnFrameChanged;
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
            if (_storage.TryLoad(out var loaded))
                _state = loaded;
            else
                _state = new AvatarUserState();

            // Repair defaults
            if (string.IsNullOrWhiteSpace(_state.userName)) _state.userName = "Player";

            if (_state.ownedAvatarIds == null) _state.ownedAvatarIds = new List<string>();
            GrantFreeAvatarId();

            if (_state.ownedFrameIds == null) _state.ownedFrameIds = new List<string>();
            GrantFreeFrameId();

            // chọn default avatar nếu chưa có
            if (string.IsNullOrWhiteSpace(_state.selectedAvatarId)) _state.selectedAvatarId = GetDefaultAvatarIdFromDb();
            if (string.IsNullOrWhiteSpace(_state.selectedFrameId)) _state.selectedFrameId = GetDefaultFrameIdFromDb();

            EnsureOwnedSelected();
            EnsureOwnedSelectedFrame();

            _storage.TrySave(_state);

            _initialized = true;
            OnInitialized?.Invoke();
            return AvatarResult.Ok();

            //// Ensure default owned
            //var def = _db.GetDefaultOrFirst();
            //if (def != null)
            //{
            //    var defId = def.AvatarId;

            //    // Default avatars should be owned by default (common casual pattern).
            //    if (!_state.Owns(defId))
            //        _state.AddOwned(defId);

            //    // Ensure selected exists
            //    if (string.IsNullOrWhiteSpace(_state.selectedAvatarId))
            //        _state.selectedAvatarId = defId.Value;
            //}

            //// Repair selection if invalid
            //var currentId = new AvatarId(_state.selectedAvatarId);
            //if (!currentId.IsValid || !_db.TryGet(currentId, out _))
            //{
            //    var fallback = _db.GetDefaultOrFirst();
            //    _state.selectedAvatarId = fallback != null ? fallback.id : string.Empty;
            //}

            //// Save back repaired state
            //if (!_storage.TrySave(_state))
            //{
            //    _log.Warn("AvatarService: storage save failed during Initialize.");
            //}

            //_initialized = true;
            //OnInitialized?.Invoke();

            //return AvatarResult.Ok();
        }

        private void GrantFreeFrameId()
        {
            if (_db.frames != null)
            {
                for (int i = 0; i < _db.frames.Count; i++)
                {
                    var a = _db.frames[i];
                    if (a == null) continue;
                    if (string.IsNullOrWhiteSpace(a.id)) continue;

                    var t = a.unlockType;

                    // ✅ rule: Free always owned
                    // ✅ rule (optional): Default also owned if you treat it as starter-free
                    if (t == AvatarUnlockType.Free || t == AvatarUnlockType.Default)
                    {
                        if (!_state.ownedFrameIds.Contains(a.id))
                            _state.ownedFrameIds.Add(a.id);
                    }
                }
            }
        }

        private void GrantFreeAvatarId()
        {
            // Grant all Free (and optionally Default) avatars as owned at init
            if (_db.avatars != null)
            {
                for (int i = 0; i < _db.avatars.Count; i++)
                {
                    var a = _db.avatars[i];
                    if (a == null) continue;
                    if (string.IsNullOrWhiteSpace(a.id)) continue;

                    var t = a.unlockType;

                    // ✅ rule: Free always owned
                    // ✅ rule (optional): Default also owned if you treat it as starter-free
                    if (t == AvatarUnlockType.Free || t == AvatarUnlockType.Default)
                    {
                        if (!_state.ownedAvatarIds.Contains(a.id))
                            _state.ownedAvatarIds.Add(a.id);
                    }
                }
            }
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
                userName = _state.userName,
                selectedAvatarId = _state.selectedAvatarId,
                selectedFrameId = _state.selectedFrameId,

                ownedAvatarIds = new List<string>(_state.ownedAvatarIds ?? new List<string>()),
                ownedFrameIds = new List<string>(_state.ownedFrameIds ?? new List<string>())
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

        public AvatarResult<AvatarId> GetSelectedFrameId()
        {
            if (!_initialized) return AvatarResult<AvatarId>.Fail(AvatarError.NotInitialized, "Not initialized.");
            var id = new AvatarId(_state.selectedFrameId);
            if (!id.IsValid) return AvatarResult<AvatarId>.Fail(AvatarError.InvalidId, "Selected frame id invalid.");
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

        public AvatarResult<AvatarOwnershipState> GetFrameState(AvatarId id)
        {
            if (!_initialized) return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGetFrame(id, out var def))
                return AvatarResult<AvatarOwnershipState>.Fail(AvatarError.NotFoundInDatabase, "Not in database.", AvatarOwnershipState.Unknown);

            var selected = string.Equals(_state.selectedFrameId, id.Value, StringComparison.Ordinal);
            var owned = _state.OwnFrame(id);

            if (selected && owned) return AvatarResult<AvatarOwnershipState>.Ok(AvatarOwnershipState.Selected);
            if (owned) return AvatarResult<AvatarOwnershipState>.Ok(AvatarOwnershipState.Owned);

            var canUnlock = _unlockProvider.CanUnlock(def, _state, out _);
            return AvatarResult<AvatarOwnershipState>.Ok(canUnlock ? AvatarOwnershipState.Unlockable : AvatarOwnershipState.Locked);
        }

        public AvatarResult SaveProfile()
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");

            return _storage.TrySave(_state)
                    ? AvatarResult.Ok()
                    : AvatarResult.Fail(AvatarError.StorageFailure);
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

            ////no need to change, just return ok, only invoke event when save changed
            //return AvatarResult.Ok();
            _state.selectedAvatarId = id.Value;

            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: save failed on select.");
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save selection.");
            }

            OnAvatarChanged?.Invoke(id);
            return AvatarResult.Ok();
        }

        public AvatarResult TrySelectFrame(AvatarId id) 
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGetFrame(id, out _))
                return AvatarResult.Fail(AvatarError.NotFoundInDatabase, $"Avatar '{id.Value}' not in database.");

            if (!_state.OwnFrame(id))
                return AvatarResult.Fail(AvatarError.NotOwned, "Avatar not owned.");

            if (string.Equals(_state.selectedFrameId, id.Value, StringComparison.Ordinal))
                return AvatarResult.Ok("Already selected.");

            ////no need to change, just return ok, only invoke event when save changed
            //return AvatarResult.Ok();

            _state.selectedFrameId = id.Value;

            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: save failed on select.");
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save selection.");
            }

            OnFrameChanged?.Invoke(id);
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

        public AvatarResult TryUnlockFrame(AvatarId id)
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized, "Not initialized.");
            if (!id.IsValid) return AvatarResult.Fail(AvatarError.InvalidId, "Invalid id.");

            if (!_db.TryGetFrame(id, out var def))
                return AvatarResult.Fail(AvatarError.NotFoundInDatabase, $"Frame '{id.Value}' not in database.");

            if (_state.OwnFrame(id))
                return AvatarResult.Ok("Already owned.");

            if (!_unlockProvider.CanUnlock(def, _state, out var reason))
                return AvatarResult.Fail(AvatarError.NotUnlockable, reason ?? "Not unlockable.");

            if (!_unlockProvider.TryUnlock(def, _state, out var err))
                return AvatarResult.Fail(AvatarError.UnlockProviderFailure, err ?? "Unlock failed.");

            _state.AddOwnedFrame(id);

            if (!_storage.TrySave(_state))
            {
                _log.Warn("AvatarService: save failed on unlock.");
                return AvatarResult.Fail(AvatarError.StorageFailure, "Failed to save frame unlock.");
            }

            OnInventoryChanged?.Invoke();
            return AvatarResult.Ok();
        }

        // Convenience: unlock then select (common UX)
        public AvatarResult TryUnlockAndSelect(AvatarId id)
        {
            if (_state.Owns(id)) return TrySelect(id);

            var unlock = TryUnlock(id);
            if (!unlock.ok) return unlock;

            return TrySelect(id);
        }

        public AvatarResult TryUnlockAndSelectFrame(AvatarId id)
        {
            if (_state.OwnFrame(id)) return TrySelectFrame(id);

            var unlock = TryUnlockFrame(id);
            if (!unlock.ok) return unlock;

            return TrySelectFrame(id);
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

        internal AvatarResult TryUpdateUserName(string newName)
        {
            if (!_initialized) return AvatarResult.Fail(AvatarError.NotInitialized);
            newName = (newName ?? "").Trim();
            if (newName.Length == 0) return AvatarResult.Fail(AvatarError.InvalidId, "Empty name");
            if (string.Equals(_state.userName, newName, StringComparison.Ordinal))
                return AvatarResult.Ok();
            _state.userName = newName;
            if (!_storage.TrySave(_state))
                return AvatarResult.Fail(AvatarError.StorageFailure);
            OnUserNameChanged?.Invoke(newName);
            return AvatarResult.Ok();
        }

        private string GetDefaultAvatarIdFromDb()
        {
            var def = _db.GetDefaultOrFirst();
            return def != null ? def.id : string.Empty;
        }

        private string GetDefaultFrameIdFromDb()
        {
            var def = _db.GetDefaultFrameOrFirst();
            return def != null ? def.id : string.Empty;
        }

        private void EnsureOwnedSelected()
        {
            if (!string.IsNullOrWhiteSpace(_state.selectedAvatarId) &&
                !_state.ownedAvatarIds.Contains(_state.selectedAvatarId))
            {
                _state.ownedAvatarIds.Add(_state.selectedAvatarId);
            }
        }

        private void EnsureOwnedSelectedFrame()
        {
            if (!string.IsNullOrWhiteSpace(_state.selectedFrameId) &&
                !_state.ownedFrameIds.Contains(_state.selectedFrameId))
            {
                _state.ownedFrameIds.Add(_state.selectedFrameId);
            }
        }

        internal AvatarUserState GetAvatarInfo()
        {
            return _state;
        }
    }
}
