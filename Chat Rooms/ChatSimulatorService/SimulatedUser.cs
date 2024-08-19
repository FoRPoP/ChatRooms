using Interfaces.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChatSimulatorService
{
    public class SimulatedUser
    {
        private string username;
        private string chatRoomId;
        private RegionsEnum chatRoomRegion;
        private readonly ChatServiceClient chatServiceClient;

        public SimulatedUser()
        {
            username = GenerateRandomUsername();
            chatRoomId = string.Empty;
            chatRoomRegion = RegionsEnum.WORLD;
            chatServiceClient = new();
        }

        public async Task RegisterAndLoginAsync()
        {
            string password = GenerateRandomPassword();
            await chatServiceClient.RegisterAndLoginAsync(username, password);
        }

        public async Task JoinChatRoomAsync()
        {
            Tuple<string, string> joinedRoomInfo = await chatServiceClient.JoinChatRoomAsync();
            chatRoomId = joinedRoomInfo.Item1;
            chatRoomRegion = (RegionsEnum)Enum.Parse(typeof(RegionsEnum), joinedRoomInfo.Item2);
        }

        public async Task SimulateTypingAsync()
        {
            while(true)
            {
                Message message = GenerateRandomMessage();
                await chatServiceClient.SendMessageAsync(chatRoomId, message, chatRoomRegion);
                await Task.Delay(new Random().Next(5000, 10000));
            }
        }

        private string GenerateRandomUsername()
        {
            return "User" + new Random().Next(1000, 9999);
        }

        private string GenerateRandomPassword()
        {
            return "Password" + new Random().Next(1000, 9999);
        }

        private Message GenerateRandomMessage()
        {
            string[] words = { "The", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog", "and", "runs", "away", "into", "the", "forest", "bright", "sunny", "day", "beautiful", "flowers", "in", "the", "garden", "birds", "singing", "happy", "children", "playing", "with", "their", "toys", "under", "the", "blue", "sky" };

            Random random = new();
            StringBuilder messageText = new StringBuilder();
            for (int i = 0; i < random.Next(1, 16); i++)
            {
                messageText.Append(words[random.Next(words.Length)] + " ");
            }

            Message message = new();
            message.Text = messageText.ToString();

            return message;
        }
    }
}
