using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public sealed class PlayerPrefsAvatarStorage : IAvatarStorage
    {
        private readonly string _key;

        [Serializable]
        private class Wrapper
        {
            public string selectedAvatarId;
            public List<string> ownedAvatarIds;
        }

        public PlayerPrefsAvatarStorage(string key = "avatar_user_state_v1")
        {
            _key = string.IsNullOrWhiteSpace(key) ? "avatar_user_state_v1" : key;
        }

        public bool TryLoad(out AvatarUserState state)
        {
            state = new AvatarUserState();

            try
            {
                if (!PlayerPrefs.HasKey(_key))
                    return true; // empty state

                var json = PlayerPrefs.GetString(_key, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                    return true;

                state = JsonUtility.FromJson<AvatarUserState>(json) ?? new AvatarUserState();

                state.ownedAvatarIds ??= new List<string>();
                state.ownedFrameIds ??= new List<string>();

                return true;
            }
            catch
            {
                state = new AvatarUserState();
                return false;
            }
        }

        public bool TrySave(AvatarUserState state)
        {
            try
            {
                state ??= new AvatarUserState();
                state.ownedAvatarIds ??= new List<string>();
                state.ownedFrameIds ??= new List<string>();

                var json = JsonUtility.ToJson(state);
                PlayerPrefs.SetString(_key, json);
                PlayerPrefs.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
