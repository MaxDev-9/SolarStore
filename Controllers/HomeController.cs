using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarStore.Data;

namespace SolarStore.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        var featured = _db.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToList();
        return View(featured);
    }

    public IActionResult About() => View();
    public IActionResult Plan() => View();
    public IActionResult Contact() => View();
    public IActionResult Error() => View();
}
