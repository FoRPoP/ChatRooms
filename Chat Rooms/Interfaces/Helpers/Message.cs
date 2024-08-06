namespace Interfaces.Helpers
{
    public class Message
    {
        public string ID { get; set; }
        public string ChatRoomId { get; set; }
        public string Username { get; set; }
        public DateTime DateTimeSent { get; set; }
        public string Text { get; set; }

        public Message()
        {
            ID = string.Empty;
            ChatRoomId = string.Empty;
            Username = string.Empty;
            DateTimeSent = DateTime.Now;
            Text = string.Empty;
        }

        public Message(string messageId, string chatRoomId, string username, string text)
        {
            ID = messageId;
            ChatRoomId = chatRoomId;
            Username = username;
            DateTimeSent = DateTime.Now;
            Text = text;
        }
    }
}
