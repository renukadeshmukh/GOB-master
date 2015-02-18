namespace GetOnBoard.Core.Model
{
    public class Account
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string AccessToken { get; set; }

        public int Points { get; set; }

        public int MaxGameLimit { get; set; }

        public int Level { get; set; }

        public Account()
        {
        }
    }
}
