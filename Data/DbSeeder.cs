using SolarStore.Models;

namespace SolarStore.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Admin user
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            db.Users.Add(new User
            {
                FullName = "Admin",
                Email = "admin@solarstore.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin"
            });
        }

        // Sample products
        if (!db.Products.Any())
        {
            var products = new List<Product>
            {
                new() { Name = "MonoSun 400W Panel", Category = "Panel",
                    Description = "High-efficiency monocrystalline 400W solar panel with 25-year warranty.",
                    Price = 299.99m, Stock = 50,
                    ImageUrl = "https://images.unsplash.com/photo-1509391366360-2e959784a276?w=400" },
                new() { Name = "PolyGreen 300W Panel", Category = "Panel",
                    Description = "Reliable polycrystalline 300W panel, great value for residential use.",
                    Price = 199.99m, Stock = 80,
                    ImageUrl = "https://images.unsplash.com/photo-1497440001374-f26997328c1b?w=400" },
                new() { Name = "SolarMax 5kW Inverter", Category = "Inverter",
                    Description = "Pure sine wave inverter with built-in MPPT charge controller.",
                    Price = 549.00m, Stock = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400" },
                new() { Name = "PowerWall 10kWh Battery", Category = "Battery",
                    Description = "Lithium iron phosphate home battery. 6000+ cycle life.",
                    Price = 1299.00m, Stock = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1618766939770-58e5f9498f8b?w=400" },
                new() { Name = "Roof Mount Kit", Category = "Accessory",
                    Description = "Universal aluminum mounting rail kit for up to 8 panels.",
                    Price = 89.99m, Stock = 100,
                    ImageUrl = "https://images.unsplash.com/photo-1466611653911-95081537e5b7?w=400" },
                new() { Name = "MC4 Connector Pack (10 pairs)", Category = "Accessory",
                    Description = "Waterproof MC4 solar connectors, UV resistant.",
                    Price = 19.99m, Stock = 200,
                    ImageUrl = "https://images.unsplash.com/photo-1620714223084-8fcacc2dfd4d?w=400" },
            };
            db.Products.AddRange(products);
        }

        db.SaveChanges();
    }
}
