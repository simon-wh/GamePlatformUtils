using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils.Steam
{
    public static class SteamConstants
    {
        public const string SteamAppsDirectory =
#if LINUX
            "steamapps"
#else
            "SteamApps"
#endif
            ;
    }
}
