using Microsoft.AspNetCore.Mvc;
using SolarStore.Data;

namespace SolarStore.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _db;
    public ProductController(AppDbContext db) => _db = db;

    public IActionResult Index(string? category, string? search)
    {
        var q = _db.Products.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(category))
            q = q.Where(p => p.Category == category);

        if (!string.IsNullOrEmpty(search))
            q = q.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        ViewBag.Category = category;
        ViewBag.Search = search;
        ViewBag.Categories = new[] { "Panel", "Inverter", "Battery", "Accessory" };
        return View(q.OrderBy(p => p.Name).ToList());
    }

    public IActionResult Details(int id)
    {
        var product = _db.Products.Find(id);
        if (product == null || !product.IsActive) return NotFound();
        return View(product);
    }
}
