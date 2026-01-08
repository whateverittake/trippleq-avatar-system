using System;
using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    [CreateAssetMenu(
    menuName = "TrippleQ/Avatar System/Icon Library",
    fileName = "AvatarIconLibrary")]
    public class AvatarIconLibrarySO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string iconKey;
            public Sprite sprite;
        }

        public Entry[] entries;
        public Entry[] entriesFrame;

        public Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key) || entries == null)
                return null;

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].iconKey == key)
                    return entries[i].sprite;
            }

            return null;
        }

        public Sprite GetFrame(string key)
        {
            if (string.IsNullOrEmpty(key) || entriesFrame == null)
                return null;
            for (int i = 0; i < entriesFrame.Length; i++)
            {
                if (entriesFrame[i].iconKey == key)
                    return entriesFrame[i].sprite;
            }
            return null;
        }
    }
}
