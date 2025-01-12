using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using applicat.Data;
using applicat.Models;

namespace applicat.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateReview(int gameId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            ViewBag.Game = game;
            return View();
        }

        // Сохранение отзыва в базу данных
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(int gameId, string content, int rating)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Проверяем, добавлена ли игра в список пользователя
            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.GameId == gameId && ug.UserId == int.Parse(userId));

            if (userGame == null)
            {
                TempData["Error"] = "Вы не можете оставить отзыв на игру, которой нет в вашем списке.";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            // Создаём новый отзыв
            var review = new Review
            {
                GameId = gameId,
                UserId = int.Parse(userId),
                Content = content,
                Rating = rating,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ваш отзыв успешно добавлен.";
            return RedirectToAction("GameReviews", new { gameId });
        }

        // Метод для просмотра всех отзывов к игре
        public async Task<IActionResult> GameReviews(int gameId)
        {
            var game = await _context.Games
                .Include(g => g.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }
    }
}
