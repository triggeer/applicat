namespace applicat.Models
{
    public class UserGame
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public string Status { get; set; } = "not started";
        public User? User { get; set; }
        public Game? Game { get; set; }
    }
}