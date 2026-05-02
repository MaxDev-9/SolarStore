using System.ComponentModel.DataAnnotations;

namespace SolarStore.Models;

// ── USER ─────────────────────────────────────────────────────────────────────
public class User
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FullName { get; set; } = "";

    [Required, StringLength(120)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = "Customer"; // "Admin" | "Customer"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// ── PRODUCT ───────────────────────────────────────────────────────────────────
public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string Description { get; set; } = "";

    [Required]
    public string Category { get; set; } = "Panel"; // Panel | Inverter | Battery | Accessory

    [Range(0, 9_999_999)]
    public decimal Price { get; set; }

    [Range(0, 99_999)]
    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

// ── ORDER ─────────────────────────────────────────────────────────────────────
public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending"; // Pending | Processing | Shipped | Delivered | Cancelled

    public string ShippingAddress { get; set; } = "";

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

// ── ORDER ITEM ────────────────────────────────────────────────────────────────
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// ── VIEW MODELS ───────────────────────────────────────────────────────────────
public class RegisterVm
{
    [Required, StringLength(80)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class PlaceOrderVm
{
    [Required]
    public string ShippingAddress { get; set; } = "";
    public string? Notes { get; set; }
    public List<CartItem> Cart { get; set; } = new();
}

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
