namespace applicat.Models
{
    public class UserGameViewModel
    {
        public int UserGameId { get; set; } // ID связи пользователя с игрой
        public int GameId { get; set; } // Оригинальный ID игры
        public string Title { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Запланировано";
    }


}
