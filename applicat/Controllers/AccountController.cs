using Microsoft.AspNetCore.Mvc;
using applicat.Data;
using applicat.Models;
using System.Security.Cryptography;
using System.Text;

namespace applicat.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            return View();
        }

        // Сохраняем данные пользователя в сессии
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role);

        // Теперь админ тоже попадает на главную страницу
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Все поля обязательны.");
            return View();
        }

        if (_context.Users.Any(u => u.Username == username))
        {
            ModelState.AddModelError("", "Имя пользователя уже существует.");
            return View();
        }

        // Проверяем, есть ли уже пользователь с таким email
        if (_context.Users.Any(u => u.Email == email))
        {
            ModelState.AddModelError("", "Пользователь с таким Email уже существует.");
            return View();
        }

        // Создаём нового пользователя
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = "User"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // Автоматический вход после регистрации
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.Role);

        return RedirectToAction("Index", "Games");
    }



    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Очистка сессии при выходе
        return RedirectToAction("Login");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string enteredPassword, string storedHash)
    {
        var enteredHash = HashPassword(enteredPassword);
        return enteredHash == storedHash;
    }
}
