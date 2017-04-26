namespace GamePlatformUtils.Steam
{
    //Implement ID conversion and change to struct
    public class SteamID
    {
        public readonly int ID32;
        public readonly ulong ID64;

        public SteamID(ulong id64)
        {
            ID64 = id64;
        }

        public SteamID(int id32)
        {
            this.ID32 = id32;
        }

        public override int GetHashCode()
        {
            return ID64.GetHashCode();
        }
    }

    public class SteamUser
    {
        public Steam Parent { get; set; }

        public SteamID UserID { get; set; }

        public SteamUser(ulong id64)
        {
            this.UserID = new SteamID(id64);
        }

        public SteamUser(int id32)
        {
            this.UserID = new SteamID(id32);
        }

        public SteamUser(SteamID userID)
        {
            this.UserID = userID;
        }

        public override int GetHashCode()
        {
            return UserID.GetHashCode();
        }
    }
}
