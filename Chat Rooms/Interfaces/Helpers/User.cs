namespace Interfaces.Helpers
{
    public class User
    {
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public UserInfo UserInfo { get; set; }

        public User()
        {
            Username = string.Empty;
            HashedPassword = string.Empty;
            UserInfo = new();
        }

        public User(string username, string hashedPassword, UserInfo userInfo)
        {
            Username = username;
            HashedPassword = hashedPassword;
            UserInfo = userInfo;
        }
    }
}
