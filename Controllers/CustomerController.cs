using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarStore.Data;
using SolarStore.Models;

namespace SolarStore.Controllers;

public class CustomerController : Controller
{
    private readonly AppDbContext _db;
    public CustomerController(AppDbContext db) => _db = db;

    private int? UserId => HttpContext.Session.GetInt32("UserId");
    private bool IsLoggedIn => UserId != null;

    // ── MY ORDERS (Read) ──────────────────────────────────────────────────────
    public IActionResult Orders()
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var orders = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .Where(o => o.UserId == UserId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();
        return View(orders);
    }

    // ── ORDER DETAILS (Read) ──────────────────────────────────────────────────
    public IActionResult OrderDetails(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var order = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .FirstOrDefault(o => o.Id == id && o.UserId == UserId);
        if (order == null) return NotFound();
        return View(order);
    }

    // ── PLACE ORDER (Create) ──────────────────────────────────────────────────
    [HttpGet]
    public IActionResult PlaceOrder(int productId, int qty = 1)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var product = _db.Products.Find(productId);
        if (product == null || !product.IsActive) return NotFound();

        ViewBag.Product = product;
        ViewBag.Qty = qty;
        return View();
    }

    [HttpPost]
    public IActionResult PlaceOrder(int productId, int qty, string shippingAddress, string? notes)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");

        var product = _db.Products.Find(productId);
        if (product == null || product.Stock < qty)
        {
            TempData["Error"] = "Product unavailable or insufficient stock.";
            return RedirectToAction("Index", "Product");
        }

        var order = new Order
        {
            UserId = UserId!.Value,
            ShippingAddress = shippingAddress,
            Notes = notes,
            TotalAmount = product.Price * qty,
            OrderItems = new List<OrderItem>
            {
                new() { ProductId = product.Id, Quantity = qty, UnitPrice = product.Price }
            }
        };

        product.Stock -= qty;
        _db.Orders.Add(order);
        _db.SaveChanges();

        TempData["Success"] = "Order placed successfully!";
        return RedirectToAction("OrderDetails", new { id = order.Id });
    }

    // ── EDIT ORDER (Update) – only while Pending ──────────────────────────────
    [HttpGet]
    public IActionResult EditOrder(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var order = _db.Orders
            .Include(o => o.OrderItems).ThenInclude(i => i.Product)
            .FirstOrDefault(o => o.Id == id && o.UserId == UserId);
        if (order == null || order.Status != "Pending") return NotFound();
        return View(order);
    }

    [HttpPost]
    public IActionResult EditOrder(int id, string shippingAddress, string? notes)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var order = _db.Orders.FirstOrDefault(o => o.Id == id && o.UserId == UserId);
        if (order == null || order.Status != "Pending") return NotFound();

        order.ShippingAddress = shippingAddress;
        order.Notes = notes;
        _db.SaveChanges();

        TempData["Success"] = "Order updated.";
        return RedirectToAction("OrderDetails", new { id });
    }

    // ── CANCEL ORDER (Delete-like) – only while Pending ───────────────────────
    [HttpPost]
    public IActionResult CancelOrder(int id)
    {
        if (!IsLoggedIn) return RedirectToAction("Login", "Account");
        var order = _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefault(o => o.Id == id && o.UserId == UserId);
        if (order == null || order.Status != "Pending") return NotFound();

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            var product = _db.Products.Find(item.ProductId);
            if (product != null) product.Stock += item.Quantity;
        }

        order.Status = "Cancelled";
        _db.SaveChanges();

        TempData["Success"] = "Order cancelled.";
        return RedirectToAction("Orders");
    }
}
