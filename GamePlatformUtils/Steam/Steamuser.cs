using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils.Steam
{
    public class SteamUser
    {
        public ulong UserID { get; set; }

        public SteamUser(ulong UserID)
        {
            this.UserID = UserID;
        }

    }
}
