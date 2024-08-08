namespace Interfaces.Helpers
{
    public class UserInfo
    {
        public List<Message> MessagesSent { get; set; }
        public HashSet<string> FavouritedChatsIds { get; set; }
        public List<string> CreatedChatsIds { get; set; }

        public UserInfo()
        {
            MessagesSent = new();
            FavouritedChatsIds = new();
            CreatedChatsIds = new();
        }

        public UserInfo(List<Message> messagesSent, HashSet<string> favouritedChatsIds, List<string> createdChatsIds)
        {
            MessagesSent = messagesSent;
            FavouritedChatsIds = favouritedChatsIds;
            CreatedChatsIds = createdChatsIds;
        }
    }
}
