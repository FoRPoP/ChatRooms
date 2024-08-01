namespace Interfaces.Helpers
{
    public class ChatData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerId { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public Dictionary<string, string> ActiveChatters { get; set; }

        public ChatData()
        {
            ID = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            OwnerId = string.Empty;
            DateTimeCreated = DateTime.Now;
            ActiveChatters = new();
        }

        public ChatData(string chatId, string name, string description, string ownerId)
        {
            ID = chatId;
            Name = name;
            Description = description;
            OwnerId = ownerId;
            DateTimeCreated = DateTime.Now;
            ActiveChatters = new();
        }
    }
}
