﻿namespace Interfaces.Helpers
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

        public void AddChatter(string userId)
        {
            ChatData.ActiveChatters.Add(userId);
        }

        public void RemoveChatter(string userId)
        {
            ChatData.ActiveChatters.Remove(userId);
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
            ChatData.TotalMessages++;
        }
    }
}
