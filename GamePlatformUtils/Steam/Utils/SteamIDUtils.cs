using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils.Steam.Utils
{
    public static class SteamIDUtils
    {
        public const ulong IDConstant = 76561197960265728;

        public static int ID64ToID32(ulong id64)
        {
            return (int)(id64 - IDConstant);
        }

        public static ulong ID32ToID64(int id32)
        {
            return (ulong)id32 + IDConstant;
        }
    }
}
