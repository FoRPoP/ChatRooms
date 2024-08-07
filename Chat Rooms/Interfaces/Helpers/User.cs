namespace Interfaces.Helpers
{
    public class User
    {
        public string Username { get; set; }
        public string HashedPassword { get; set; }

        public User()
        {
            Username = string.Empty;
            HashedPassword = string.Empty;
        }

        public User(string username, string hashedPassword)
        {
            Username = username;
            HashedPassword = hashedPassword;
        }
    }
}
