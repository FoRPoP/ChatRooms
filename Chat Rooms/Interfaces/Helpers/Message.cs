namespace Interfaces.Helpers
{
    public class Message
    {
        public string ID { get; set; }
        public string ChatRoomId { get; set; }
        public string UserId { get; set; }
        public DateTime DateTimeSent { get; set; }
        public string Text { get; set; }

        public Message()
        {
            ID = string.Empty;
            ChatRoomId = string.Empty;
            UserId = string.Empty;
            DateTimeSent = DateTime.Now;
            Text = string.Empty;
        }

        public Message(string messageId, string chatRoomId, string userId, string text)
        {
            ID = messageId;
            ChatRoomId = chatRoomId;
            UserId = userId;
            DateTimeSent = DateTime.Now;
            Text = text;
        }
    }
}
