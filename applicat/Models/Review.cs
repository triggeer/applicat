namespace applicat.Models
{
    public class Review
    {
        public int Id { get; set; }

        // Связь с пользователем
        public int UserId { get; set; }
        public User User { get; set; }

        // Связь с игрой
        public int GameId { get; set; }
        public Game Game { get; set; }

        // Контент отзыва
        public string Content { get; set; } = string.Empty;

        // Рейтинг (1-5 звезд)
        public int Rating { get; set; }

        // Дата создания
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
