using Microsoft.EntityFrameworkCore;
using SolarStore.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Convert Railway's DATABASE_URL (postgresql://user:pass@host:port/db)
var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connStr;
if (!string.IsNullOrEmpty(rawUrl))
{
    var uri = new Uri(rawUrl);
    var userInfo = uri.UserInfo.Split(':');
    connStr = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connStr = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No database connection string found.");
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connStr));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(4);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Create tables directly via raw SQL — bypasses migration history issues
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""Users"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""FullName"" VARCHAR(80) NOT NULL,
            ""Email"" VARCHAR(120) NOT NULL,
            ""PasswordHash"" TEXT NOT NULL,
            ""Role"" TEXT NOT NULL,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");

        CREATE TABLE IF NOT EXISTS ""Products"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""Name"" VARCHAR(120) NOT NULL,
            ""Description"" VARCHAR(1000) NOT NULL,
            ""Category"" TEXT NOT NULL,
            ""Price"" NUMERIC(18,2) NOT NULL,
            ""Stock"" INT NOT NULL,
            ""ImageUrl"" TEXT,
            ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );

        CREATE TABLE IF NOT EXISTS ""Orders"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""UserId"" INT NOT NULL REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
            ""OrderDate"" TIMESTAMP NOT NULL DEFAULT NOW(),
            ""Status"" TEXT NOT NULL,
            ""ShippingAddress"" TEXT NOT NULL,
            ""TotalAmount"" NUMERIC(18,2) NOT NULL,
            ""Notes"" TEXT
        );
        CREATE INDEX IF NOT EXISTS ""IX_Orders_UserId"" ON ""Orders"" (""UserId"");

        CREATE TABLE IF NOT EXISTS ""OrderItems"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""OrderId"" INT NOT NULL REFERENCES ""Orders""(""Id"") ON DELETE CASCADE,
            ""ProductId"" INT NOT NULL REFERENCES ""Products""(""Id"") ON DELETE CASCADE,
            ""Quantity"" INT NOT NULL,
            ""UnitPrice"" NUMERIC(18,2) NOT NULL
        );
        CREATE INDEX IF NOT EXISTS ""IX_OrderItems_OrderId"" ON ""OrderItems"" (""OrderId"");
        CREATE INDEX IF NOT EXISTS ""IX_OrderItems_ProductId"" ON ""OrderItems"" (""ProductId"");
    ");

    DbSeeder.Seed(db);
}

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
