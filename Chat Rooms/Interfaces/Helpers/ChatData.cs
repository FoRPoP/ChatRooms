﻿namespace Interfaces.Helpers
{
    public class ChatData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OwnerUsername { get; set; }
        public RegionsEnum Region { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public List<string> ActiveChatters { get; set; }
        public int TotalMessages { get; set; }

        public ChatData()
        {
            ID = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            OwnerUsername = string.Empty;
            Region = RegionsEnum.WORLD;
            DateTimeCreated = DateTime.Now;
            ActiveChatters = new();
            TotalMessages = 0;
        }

        public ChatData(string chatId, string name, string description, string ownerUsername, RegionsEnum region)
        {
            ID = chatId;
            Name = name;
            Description = description;
            OwnerUsername = ownerUsername;
            Region = region;
            DateTimeCreated = DateTime.Now;
            ActiveChatters = new();
            TotalMessages = 0;
        }
    }
}
