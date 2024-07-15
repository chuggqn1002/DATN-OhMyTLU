namespace OhMyTLU.Hubs
{
    public static class UserDic
    {
        public static IDictionary<string, string> listPeer = new Dictionary<string, string>();
        public static IDictionary<string, string> listUserId = new Dictionary<string, string>();
        public static List<string> waittingRoom = new List<string>();
    }

}
