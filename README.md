# ☀️ Solar Store – ASP.NET MVC 8 + SQLite

Full-stack solar e-commerce with **Customer CRUD**, **Admin panel**, **Sign In / Sign Up** with roles.

## 🔑 Default Admin Credentials
```
Email:    admin@solarstore.com
Password: Admin@123
```

---

## 🏃 Run Locally

```bash
# 1. Install .NET 8 SDK from https://dotnet.microsoft.com/download

# 2. Restore & run
cd SolarStore
dotnet run

# 3. Open http://localhost:5000
```

---

## 🚀 Deploy to Railway

### Option A – GitHub (recommended)

1. Push this folder to a **GitHub repo**
2. Go to [railway.app](https://railway.app) → **New Project** → **Deploy from GitHub repo**
3. Select your repo — Railway auto-detects the `Dockerfile`
4. Add a **Volume** (for SQLite persistence):
   - Go to your service → **Volumes** → Add Volume
   - Mount path: `/data`
5. Click **Deploy** — done! 🎉

### Option B – Railway CLI

```bash
npm install -g @railway/cli
railway login
railway init
railway up
```

### Environment Variables (optional)
| Key | Default | Description |
|-----|---------|-------------|
| `DB_PATH` | `/data/solar.db` | SQLite file location |
| `PORT` | `8080` | Auto-set by Railway |

---

## 📁 Project Structure

```
SolarStore/
├── Controllers/
│   ├── HomeController.cs      # Public pages
│   ├── AccountController.cs   # Login / Register / Profile
│   ├── ProductController.cs   # Customer browsing
│   ├── CustomerController.cs  # Customer CRUD (orders)
│   └── AdminController.cs     # Admin CRUD (products/users/orders)
├── Models/
│   └── Models.cs              # User, Product, Order, OrderItem + ViewModels
├── Data/
│   ├── AppDbContext.cs        # EF Core DbContext
│   └── DbSeeder.cs            # Seeds admin + sample products
├── Views/
│   ├── Account/               # Login, Register, Profile
│   ├── Admin/                 # Dashboard, Products, Users, Orders
│   ├── Customer/              # Orders, OrderDetails, PlaceOrder, EditOrder
│   ├── Home/                  # Index, About, Plan, Contact
│   ├── Product/               # Index, Details
│   └── Shared/                # _Layout, _AdminLayout
├── Migrations/                # EF Core migrations (SQLite)
├── wwwroot/css/site.css       # Solar Store theme
├── Dockerfile                 # Railway deployment
└── railway.toml               # Railway config
```

## ✨ Features

| Feature | Details |
|---------|---------|
| **Auth** | BCrypt-hashed passwords, session-based roles |
| **Customer CRUD** | Place / View / Edit / Cancel orders |
| **Admin Panel** | Full CRUD for Products, Users; order status management |
| **SQLite** | Zero-config DB, persisted via Railway Volume |
| **Auto-migrate** | DB schema created + seeded on first run |
