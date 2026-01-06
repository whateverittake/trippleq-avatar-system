using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public class AvatarSystemBootstrap : MonoBehaviour
    {
        [SerializeField] private AvatarDatabaseSO _db;
        [SerializeField] AvatarIconLibrarySO _iconLibrary;

        private AvatarService _service;
        public static AvatarService Service { get; private set; }

        void Start()
        {
            _service = new AvatarService();

            var storage = new PlayerPrefsAvatarStorage("mygame_avatar_v1");
            var unlock = new BasicAvatarUnlockProvider(); // sau này project implement thật
            var res = _service.Initialize(_db, storage, unlock);

            Debug.Log($"Init ok={res.ok} err={res.error}");

            AvatarServiceLocator.Provide(_service);
            AvatarIconResolver.SetLibrary(_iconLibrary);
        }
    }
}
