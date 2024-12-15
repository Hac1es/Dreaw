namespace SignalRServer.Models
{
    public class UserModel
    {
        public string? Username { get; set; }
        public required string Email {  get; set; }
        public string? Password { get; set; }
        public string? UserID { get; set; }
    }
}
