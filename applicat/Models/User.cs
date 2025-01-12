﻿namespace applicat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Хэш пароля
        public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>(); // Игры пользователя

        public string Role { get; set; } = "User";

    }
}
