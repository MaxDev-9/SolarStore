using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarStore.Data;
using SolarStore.Models;

namespace SolarStore.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    private bool IsAdmin =>
        HttpContext.Session.GetString("Role") == "Admin";

    private IActionResult Guard() =>
        IsAdmin ? null! : RedirectToAction("Login", "Account");

    // ══════════════════════════════════════════════════════════════════════════
    // DASHBOARD
    // ══════════════════════════════════════════════════════════════════════════
    public IActionResult Index()
    {
        if (!IsAdmin) return RedirectToAction("Login", "Account");
        ViewBag.TotalProducts = _db.Products.Count();
        ViewBag.TotalUsers = _db.Users.Count(u => u.Role == "Customer");
        ViewBag.TotalOrders = _db.Orders.Count();
        ViewBag.Revenue = _db.Orders
            .Where(o => o.Status != "Cancelled")
            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
        ViewBag.RecentOrders = _db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToList();
        return View();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // PRODUCTS CRUD
    // ══════════════════════════════════════════════════════════════════════════
    public IActionResult Products()
    {
        var g = Guard(); if (g != null) return g;
        return View(_db.Products.OrderByDescending(p => p.CreatedAt).ToList());
    }

    [HttpGet]
    public IActionResult CreateProduct()
    {
        var g = Guard(); if (g != null) return g;
        return View(new Product());
    }

    [HttpPost]
    public IActionResult CreateProduct(Product p)
    {
        var g = Guard(); if (g != null) return g;
        if (!ModelState.IsValid) return View(p);
        _db.Products.Add(p);
        _db.SaveChanges();
        TempData["Success"] = "Product created.";
        return RedirectToAction("Products");
    }

    [HttpGet]
    public IActionResult EditProduct(int id)
    {
        var g = Guard(); if (g != null) return g;
        var p = _db.Products.Find(id);
        if (p == null) return NotFound();
        return View(p);
    }

    [HttpPost]
    public IActionResult EditProduct(Product updated)
    {
        var g = Guard(); if (g != null) return g;
        if (!ModelState.IsValid) return View(updated);
        var p = _db.Products.Find(updated.Id);
        if (p == null) return NotFound();

        p.Name = updated.Name;
        p.Description = updated.Description;
        p.Category = updated.Category;
        p.Price = updated.Price;
        p.Stock = updated.Stock;
        p.ImageUrl = updated.ImageUrl;
        p.IsActive = updated.IsActive;
        _db.SaveChanges();
        TempData["Success"] = "Product updated.";
        return RedirectToAction("Products");
    }

    [HttpPost]
    public IActionResult DeleteProduct(int id)
    {
        var g = Guard(); if (g != null) return g;
        var p = _db.Products.Find(id);
        if (p != null) { _db.Products.Remove(p); _db.SaveChanges(); }
        TempData["Success"] = "Product deleted.";
        return RedirectToAction("Products");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // USERS CRUD
    // ══════════════════════════════════════════════════════════════════════════
    public IActionResult Users()
    {
        var g = Guard(); if (g != null) return g;
        return View(_db.Users.OrderByDescending(u => u.CreatedAt).ToList());
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        var g = Guard(); if (g != null) return g;
        var u = _db.Users.Find(id);
        if (u == null) return NotFound();
        return View(u);
    }

    [HttpPost]
    public IActionResult EditUser(User updated, string? newPassword)
    {
        var g = Guard(); if (g != null) return g;
        var u = _db.Users.Find(updated.Id);
        if (u == null) return NotFound();

        u.FullName = updated.FullName;
        u.Email = updated.Email;
        u.Role = updated.Role;
        if (!string.IsNullOrEmpty(newPassword))
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        _db.SaveChanges();
        TempData["Success"] = "User updated.";
        return RedirectToAction("Users");
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var g = Guard(); if (g != null) return g;
        var u = _db.Users.Find(id);
        if (u != null) { _db.Users.Remove(u); _db.SaveChanges(); }
        TempData["Success"] = "User deleted.";
        return RedirectToAction("Users");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ORDERS MANAGEMENT
    // ══════════════════════════════════════════════════════════════════════════
    public IActionResult Orders(string? status)
    {
        var g = Guard(); if (g != null) return g;
        var q = _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            q = q.Where(o => o.Status == status);

        ViewBag.StatusFilter = status;
        return View(q.OrderByDescending(o => o.OrderDate).ToList());
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int id, string status)
    {
        var g = Guard(); if (g != null) return g;
        var order = _db.Orders.Find(id);
        if (order == null) return NotFound();
        order.Status = status;
        _db.SaveChanges();
        TempData["Success"] = $"Order #{id} marked as {status}.";
        return RedirectToAction("Orders");
    }
}
