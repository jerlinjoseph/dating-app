namespace DatingApp.API.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }

        public byte[] Passwordhash { get; set; }

        public byte[] Passwordsalt {get; set;}
    }
}