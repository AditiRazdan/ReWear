using LocalBakery.Data;
using LocalBakery.Models;
using LocalBakery.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var isEfDesignTime = Environment.CommandLine.Contains("ef.dll", StringComparison.OrdinalIgnoreCase);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
    options.Conventions.AllowAnonymousToPage("/Admin/Register");
    options.Conventions.AllowAnonymousToPage("/Admin/Logout");
    options.Conventions.AuthorizeFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Register");
    options.Conventions.AllowAnonymousToPage("/Account/Logout");
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=rewear.db"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddSingleton<ImageStorage>();
builder.Services.AddScoped<UserService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("ok"));

app.MapRazorPages();

if (!isEfDesignTime)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LocalBakery.Data.AppDbContext>();
    var users = scope.ServiceProvider.GetRequiredService<UserService>();

    db.Database.Migrate();

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS AppUsers (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserName TEXT NOT NULL,
            NormalizedUserName TEXT NOT NULL UNIQUE,
            PasswordHash TEXT NOT NULL,
            Role TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL
        );
        """);

    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN PickupTimeUtc TEXT");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("""
            UPDATE Orders
            SET PickupTimeUtc = datetime(CreatedAtUtc, '+30 minutes')
            WHERE PickupTimeUtc IS NULL
            """);
    }
    catch
    {
        // Ignore if the Orders table isn't available yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN SellerNote TEXT");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN CustomerUserName TEXT");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }

    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE MenuItems ADD COLUMN DailyStock INTEGER");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE MenuItems ADD COLUMN DailyStockRemaining INTEGER");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE MenuItems ADD COLUMN StockResetDateUtc TEXT");
    }
    catch
    {
        // Ignore if the column already exists or the table hasn't been created yet.
    }
    try
    {
        db.Database.ExecuteSqlRaw("""
            UPDATE MenuItems
            SET DailyStock = 0,
                DailyStockRemaining = 0,
                StockResetDateUtc = date('now', '+8 hours')
            WHERE DailyStock IS NULL OR DailyStockRemaining IS NULL OR StockResetDateUtc IS NULL
            """);
    }
    catch
    {
        // Ignore if the MenuItems table isn't available yet.
    }

    if (!await users.AnyUsersAsync())
    {
        var seedUser = Environment.GetEnvironmentVariable("ADMIN_USERNAME")
                       ?? builder.Configuration["AdminSeed:UserName"]
                       ?? "admin";
        var seedPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                       ?? builder.Configuration["AdminSeed:Password"]
                       ?? "ChangeMe123!";
        await users.CreateUserAsync(seedUser, seedPass, "Admin");
    }

    var seedItems = new List<MenuItem>
    {
        new()
        {
            Name = "Black Ribbed Y2K Top (XS-S)",
            Description = "Ribbed long-sleeve with a square neckline and Y2K trim.",
            Price = 14.00m,
            Category = "Tops",
            TagsCsv = "Ribbed, Y2K, Classic",
            ImagePath = "/images/set-01-y2k-top.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Ivory Ribbed Y2K V-Neck (S)",
            Description = "Soft ivory top with a deep V and Y2K detail.",
            Price = 14.00m,
            Category = "Tops",
            TagsCsv = "Soft, Y2K, Neutral",
            ImagePath = "/images/set-02-y2k-top.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Blush Tie-Front Ribbed Top (S)",
            Description = "Ribbed long-sleeve with a wrap neckline and waist tie.",
            Price = 15.00m,
            Category = "Tops",
            TagsCsv = "Blush, Wrap, Ribbed",
            ImagePath = "/images/set-03-blush-top.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Black Y2K Wrap Top (S-M)",
            Description = "Flutter-sleeve wrap top with Y2K trim neckline.",
            Price = 16.00m,
            Category = "Tops",
            TagsCsv = "Wrap, Y2K, Evening",
            ImagePath = "/images/set-04-y2k-wrap-top.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Union Jack Pocket Denim Shorts (S)",
            Description = "Distressed denim shorts with bold flag back pockets.",
            Price = 18.00m,
            Category = "Bottoms",
            TagsCsv = "Denim, Statement, Vintage",
            ImagePath = "/images/set-05-denim-shorts.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Dark Wash Distressed Skinny Jeans (S)",
            Description = "Deep indigo skinny jeans with light distressing.",
            Price = 20.00m,
            Category = "Bottoms",
            TagsCsv = "Denim, Skinny, Distressed",
            ImagePath = "/images/set-06-skinny-jeans.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Classic Mid-Wash Jeans (M)",
            Description = "Straight-leg denim with a clean mid-wash finish.",
            Price = 19.00m,
            Category = "Bottoms",
            TagsCsv = "Denim, Classic, Everyday",
            ImagePath = "/images/set-07-midwash-jeans.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Navy USA Graphic Tee (S)",
            Description = "Soft navy tee with a stitched USA flag patch.",
            Price = 10.00m,
            Category = "Tops",
            TagsCsv = "Graphic, Cotton, Casual",
            ImagePath = "/images/set-08-graphic-tee.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Mocha Ribbed Tank (S)",
            Description = "Ribbed tank with wide straps in a warm brown tone.",
            Price = 8.00m,
            Category = "Tops",
            TagsCsv = "Ribbed, Neutral, Layering",
            ImagePath = "/images/set-09-ribbed-tank.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Oatmeal Joggers (M)",
            Description = "Soft fleece joggers with a cinched ankle and drawstring.",
            Price = 16.00m,
            Category = "Loungewear",
            TagsCsv = "Cozy, Neutral, Everyday",
            ImagePath = "/images/set-10-joggers.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Midnight Tie-Dye Sports Top (S)",
            Description = "Supportive sports top with a navy and lavender wash.",
            Price = 12.00m,
            Category = "Activewear",
            TagsCsv = "Tie-dye, Stretch, Active",
            ImagePath = "/images/set-11-sports-top.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Sage High-Waist Leggings (M)",
            Description = "High-waist leggings in a soft sage green.",
            Price = 15.00m,
            Category = "Activewear",
            TagsCsv = "Sage, Stretch, Comfort",
            ImagePath = "/images/set-12-leggings.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        },
        new()
        {
            Name = "Emerald Ribbed Lounge Shorts (M)",
            Description = "Lightweight ribbed shorts with a button placket.",
            Price = 9.00m,
            Category = "Loungewear",
            TagsCsv = "Ribbed, Lounge, Green",
            ImagePath = "/images/set-13-lounge-shorts.png",
            DailyStock = 1,
            DailyStockRemaining = 1
        }
    };

    db.MenuItems.RemoveRange(db.MenuItems);
    db.MenuItems.AddRange(seedItems);

    const string fallbackImage = "/images/placeholder.svg";
    var missingImages = db.MenuItems
        .Where(m => m.ImagePath == null || m.ImagePath == "")
        .ToList();
    foreach (var item in missingImages)
        item.ImagePath = fallbackImage;

    db.SaveChanges();
}

if (!isEfDesignTime)
{
    app.Run();
}
