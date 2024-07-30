namespace Interfaces.Helpers
{
    public class Chat
    {
        public ChatData ChatData { get; set; }
        public List<Message> Messages { get; set; }

        public Chat()
        {
            ChatData = new();
            Messages = [];
        }

        public Chat(ChatData chatData)
        {
            ChatData = chatData;
            Messages = [];
        }

    }
}
