using Microsoft.AspNetCore.Mvc;
using SolarStore.Data;
using SolarStore.Models;

namespace SolarStore.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    // ── REGISTER ──────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (_db.Users.Any(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Email already registered.");
            return View(vm);
        }

        var user = new User
        {
            FullName = vm.FullName,
            Email = vm.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
            Role = "Customer"
        };
        _db.Users.Add(user);
        _db.SaveChanges();

        SetSession(user);
        return RedirectToAction("Index", "Home");
    }

    // ── LOGIN ─────────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = _db.Users.FirstOrDefault(u => u.Email == vm.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        SetSession(user);

        if (user.Role == "Admin") return RedirectToAction("Index", "Admin");
        return RedirectToAction("Index", "Home");
    }

    // ── LOGOUT ────────────────────────────────────────────────────────────────
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // ── PROFILE (customer can edit own name/email/password) ──────────────────
    [HttpGet]
    public IActionResult Profile()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login");
        var user = _db.Users.Find(GetUserId());
        if (user == null) return RedirectToAction("Login");
        return View(user);
    }

    [HttpPost]
    public IActionResult Profile(User updated, string? newPassword)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login");
        var user = _db.Users.Find(GetUserId());
        if (user == null) return RedirectToAction("Login");

        // Check email uniqueness
        if (_db.Users.Any(u => u.Email == updated.Email && u.Id != user.Id))
        {
            ModelState.AddModelError("Email", "Email already in use.");
            return View(user);
        }

        user.FullName = updated.FullName;
        user.Email = updated.Email;
        if (!string.IsNullOrEmpty(newPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        _db.SaveChanges();
        HttpContext.Session.SetString("UserName", user.FullName);
        TempData["Success"] = "Profile updated.";
        return RedirectToAction("Profile");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────
    private void SetSession(User u)
    {
        HttpContext.Session.SetInt32("UserId", u.Id);
        HttpContext.Session.SetString("UserName", u.FullName);
        HttpContext.Session.SetString("Role", u.Role);
    }

    private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId") != null;
    private int GetUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
}
