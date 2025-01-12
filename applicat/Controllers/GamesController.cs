using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using applicat.Data;
using applicat.Models;

namespace applicat.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Games/Index (Список игр пользователя)
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userGames = await _context.UserGames
                .Where(ug => ug.UserId == int.Parse(userId))
                .Include(ug => ug.Game)
                .Select(ug => new UserGameViewModel
                {
                    UserGameId = ug.Id,
                    GameId = ug.Game.Id,
                    Title = ug.Game.Title,
                    Genre = ug.Game.Genre,
                    Description = ug.Game.Description,
                    Status = ug.Status
                })
                .ToListAsync();

            // Загружаем все доступные игры
            var allGames = await _context.Games.ToListAsync();
            Console.WriteLine($"ViewBag.AllGames содержит {allGames.Count} игр.");

            ViewBag.AllGames = allGames;

            return View(userGames);
        }


        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }



        // GET: Games/Create (Форма добавления игры в базу админом)
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Games/Create (Админ добавляет новую игру в базу)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Game game)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }

        // POST: Games/AddGameToUser (Пользователь добавляет игру в свой список)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGameToUser(int gameId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine($"Получен gameId: {gameId}");

            var existingGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (existingGame == null)
            {
                TempData["Error"] = "Ошибка: игра не найдена.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyAdded = await _context.UserGames.AnyAsync(ug => ug.GameId == gameId && ug.UserId == int.Parse(userId));
            if (alreadyAdded)
            {
                TempData["Error"] = "Эта игра уже есть в вашем списке.";
                return RedirectToAction(nameof(Index));
            }

            var userGame = new UserGame
            {
                UserId = int.Parse(userId),
                GameId = existingGame.Id,
                Status = "Запланировано"
            };

            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Игра успешно добавлена в ваш список!";
            return RedirectToAction(nameof(Index));
        }



        // GET: Games/Edit/5 (Редактирование игры админом)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(game);
        }

        // POST: Games/Edit/5 (Редактирование игры админом)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Description")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int userGameId, string status)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Проверяем, есть ли запись в UserGames
            var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.Id == userGameId && ug.UserId == int.Parse(userId));
            if (userGame == null)
            {
                TempData["Error"] = "Игра не найдена в вашем списке.";
                return RedirectToAction(nameof(Index));
            }

            // Разрешаем только определённые статусы
            var allowedStatuses = new List<string> { "Запланировано", "Прохожу", "Пройдено" };
            if (!allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Недопустимый статус.";
                return RedirectToAction(nameof(Index));
            }

            // Обновляем статус
            userGame.Status = status;
            _context.Update(userGame);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Статус успешно обновлён.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Games/Delete/5 (Удаление игры пользователем)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5 (Удаление игры пользователем)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ищем запись в UserGames, а не в Games
            var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.Id == id && ug.UserId == int.Parse(userId));
            if (userGame == null)
            {
                TempData["Error"] = "Игра не найдена в вашем списке.";
                return RedirectToAction(nameof(Index));
            }

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Игра удалена из вашего списка.";
            return RedirectToAction(nameof(Index));
        }


        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}
