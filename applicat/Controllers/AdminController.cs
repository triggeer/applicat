using Microsoft.AspNetCore.Mvc;
using applicat.Data;
using applicat.Models;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult ManageGames()
    {
        // Проверяем роль пользователя
        if (HttpContext.Session.GetString("UserRole") != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        var games = _context.Games.ToList();
        return View(games);
    }

    public async Task<IActionResult> ManageUsers()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    public IActionResult CreateUser()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(User user, string password)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Пароль не может быть пустым.");
                return View(user);
            }

            // Хэшируем пароль перед сохранением
            user.PasswordHash = HashPassword(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }
        return View(user);
    }
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<IActionResult> EditUser(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ManageUsers));
    }


    public IActionResult Create()
    {
        // Проверяем роль пользователя
        if (HttpContext.Session.GetString("UserRole") != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Game game)
    {
        // Проверяем роль пользователя
        if (HttpContext.Session.GetString("UserRole") != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            _context.Add(game);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageGames));
        }
        return View(game);
    }
}
