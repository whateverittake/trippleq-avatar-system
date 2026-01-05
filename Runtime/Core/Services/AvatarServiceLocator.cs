using UnityEngine;

namespace TrippleQ.AvatarSystem
{
    public static class AvatarServiceLocator
    {
        public static AvatarService Service { get; private set; }
        public static bool IsReady => Service != null && Service.IsInitialized;

        public static void Provide(AvatarService service)
        {
            Service = service;
        }
    }
}
