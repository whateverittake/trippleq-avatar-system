using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public static class AvatarIconResolver
    {
        private static AvatarIconLibrarySO _library;

        /// <summary>
        /// Called once by bootstrap or UI installer.
        /// </summary>
        public static void SetLibrary(AvatarIconLibrarySO library)
        {
            _library = library;
        }

        public static Sprite Get(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey))
                return null;

            //lookup bằng iconKey từ project library
            if (_library != null && !string.IsNullOrEmpty(iconKey))
                return _library.Get(iconKey);

            return null;
        }
    }
}
