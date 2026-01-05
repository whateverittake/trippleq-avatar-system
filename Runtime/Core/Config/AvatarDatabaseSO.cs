using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    [CreateAssetMenu(menuName = "TrippleQ/Avatar System/Avatar Database", fileName = "AvatarDatabase")]
    public sealed class AvatarDatabaseSO: ScriptableObject
    {
        public List<AvatarDefinition> avatars = new List<AvatarDefinition>();

        private Dictionary<string, AvatarDefinition> _map;

        public void BuildIndex()
        {
            _map = new Dictionary<string, AvatarDefinition>(StringComparer.Ordinal);
            if (avatars == null) return;

            for (int i = 0; i < avatars.Count; i++)
            {
                var a = avatars[i];
                if (a == null) continue;
                if (string.IsNullOrWhiteSpace(a.id)) continue;

                // last write wins
                _map[a.id] = a;
            }
        }

        public bool TryGet(AvatarId id, out AvatarDefinition def)
        {
            if (!id.IsValid)
            {
                def = null;
                return false;
            }

            if (_map == null) BuildIndex();
            return _map.TryGetValue(id.Value, out def);
        }

        public AvatarDefinition GetDefaultOrFirst()
        {
            if (avatars == null || avatars.Count == 0) return null;

            for (int i = 0; i < avatars.Count; i++)
            {
                var a = avatars[i];
                if (a != null && a.isDefault && !string.IsNullOrWhiteSpace(a.id)) return a;
            }

            // fallback first valid
            for (int i = 0; i < avatars.Count; i++)
            {
                var a = avatars[i];
                if (a != null && !string.IsNullOrWhiteSpace(a.id)) return a;
            }

            return null;
        }

#if UNITY_EDITOR
        // Optional: basic validation in editor
        public List<string> ValidateBasic()
        {
            var errors = new List<string>();
            if (avatars == null || avatars.Count == 0)
            {
                errors.Add("AvatarDatabase: avatars list is empty.");
                return errors;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            int defaultCount = 0;

            foreach (var a in avatars)
            {
                if (a == null) { errors.Add("AvatarDatabase: contains null definition."); continue; }
                if (string.IsNullOrWhiteSpace(a.id)) { errors.Add("AvatarDatabase: avatar has empty id."); continue; }
                if (!seen.Add(a.id)) errors.Add($"AvatarDatabase: duplicate id '{a.id}'.");
                if (a.isDefault) defaultCount++;
            }

            if (defaultCount == 0) errors.Add("AvatarDatabase: no default avatar (isDefault=true).");
            if (defaultCount > 1) errors.Add("AvatarDatabase: multiple default avatars. Only one should be default.");

            return errors;
        }
#endif
    }
}
