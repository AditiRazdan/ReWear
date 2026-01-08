using LocalBakery.Data;
using LocalBakery.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Account;

public class OrdersModel : PageModel
{
    private readonly AppDbContext _db;
    public OrdersModel(AppDbContext db) => _db = db;

    public List<OrderSummary> Orders { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
            return RedirectToPage("/Account/Login", new { returnUrl = "/Account/Orders" });

        var isAdmin = User.IsInRole("Admin");
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(oi => oi.MenuItem)
            .OrderByDescending(o => o.CreatedAtUtc)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(o => o.CustomerUserName == userName);

        var orders = await query
            .Take(25)
            .ToListAsync();

        Orders = orders
            .Select(o => new OrderSummary(
                o.OrderNumber,
                o.CreatedAtUtc,
                o.PickupTimeUtc,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.Items.Select(i => new OrderLine(i.MenuItem.Name, i.Quantity)).ToList()
            ))
            .ToList();

        return Page();
    }
}

public record OrderLine(string Name, int Quantity);
public record OrderSummary(string OrderNumber, DateTime CreatedAtUtc, DateTime PickupTimeUtc, decimal Total, List<OrderLine> Items);
