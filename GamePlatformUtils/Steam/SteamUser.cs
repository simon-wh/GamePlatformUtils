using GamePlatformUtils.Steam.Utils;

namespace GamePlatformUtils.Steam
{
    public class SteamID
    {
        private readonly int id32;
        private ulong? id64 = null;

        public int ID32 { get { return id32; } }
        public ulong ID64 { get { return id64 ?? (ulong)(id64 = SteamIDUtils.ID32ToID64(id32)); } }

        public SteamID(ulong id64)
        {
            id32 = SteamIDUtils.ID64ToID32(id64);
        }

        public SteamID(int id32)
        {
            this.id32 = id32;
        }

        public override int GetHashCode()
        {
            return id32.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetType().Equals(this.GetType()) && ((SteamID)obj).id32.Equals(this.id32);
        }
    }

    //Implement lookup of details for Users from steam files
    public class SteamUser
    {
        public Steam Parent { get; set; }

        public SteamID UserID { get; set; }

        private SteamUser(Steam steam)
        {
            this.Parent = steam;
        }

        public SteamUser(Steam steam, ulong id64) : this(steam)
        {
            this.UserID = new SteamID(id64);
        }

        public SteamUser(Steam steam, int id32) : this(steam)
        {
            this.UserID = new SteamID(id32);
        }

        public SteamUser(Steam steam, SteamID userID) : this(steam)
        {
            this.UserID = userID;
        }

        public override int GetHashCode()
        {
            return UserID.GetHashCode();
        }
    }
}
