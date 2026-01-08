using LocalBakery.Data;
using LocalBakery.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;
    public DashboardModel(AppDbContext db) => _db = db;

    public int TotalOrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public List<(string Name, int Qty)> TopItemsToday { get; set; } = new();
    public List<ItemStat> TopItemsLast30Days { get; set; } = new();
    public List<ItemStat> NotSellingLast30Days { get; set; } = new();
    public List<OrderSummary> RecentOrders { get; set; } = new();

    public async Task OnGetAsync()
    {
        var sgtNow = TimeHelper.GetSgtNow();
        var sgtStart = sgtNow.Date;
        var sgtEnd = sgtStart.AddDays(1);
        var today = TimeHelper.ToUtc(sgtStart);
        var tomorrow = TimeHelper.ToUtc(sgtEnd);

        TotalOrdersToday = await _db.Orders
            .Where(o => o.CreatedAtUtc >= today && o.CreatedAtUtc < tomorrow)
            .CountAsync();

        RevenueToday = await _db.OrderItems
            .Where(oi => oi.Order.CreatedAtUtc >= today && oi.Order.CreatedAtUtc < tomorrow)
            .SumAsync(oi => oi.UnitPrice * oi.Quantity);

        TopItemsToday = await _db.OrderItems
            .Where(oi => oi.Order.CreatedAtUtc >= today && oi.Order.CreatedAtUtc < tomorrow)
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .Select(x => new ValueTuple<string, int>(x.Key, x.Qty))
            .ToListAsync();

        var windowStart = TimeHelper.ToUtc(sgtStart.AddDays(-30));
        var salesLast30Days = await _db.OrderItems
            .Where(oi => oi.Order.CreatedAtUtc >= windowStart)
            .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name })
            .Select(g => new
            {
                g.Key.MenuItemId,
                g.Key.Name,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.UnitPrice * x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .ToListAsync();

        TopItemsLast30Days = salesLast30Days
            .Take(5)
            .Select(x => new ItemStat(x.Name, x.Quantity, x.Revenue))
            .ToList();

        var allItems = await _db.MenuItems
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name })
            .ToListAsync();

        var salesByItem = salesLast30Days.ToDictionary(x => x.MenuItemId, x => x.Quantity);
        NotSellingLast30Days = allItems
            .Where(item => !salesByItem.ContainsKey(item.Id))
            .Take(8)
            .Select(item => new ItemStat(item.Name, 0, 0))
            .ToList();

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(oi => oi.MenuItem)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        RecentOrders = orders
            .Select(o => new OrderSummary(
                o.OrderNumber,
                o.CreatedAtUtc,
                o.PickupTimeUtc,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.Items.Select(i => new OrderLine(i.MenuItem.Name, i.Quantity)).ToList(),
                o.SellerNote ?? string.Empty
            ))
            .ToList();
    }

    public async Task<IActionResult> OnPostClearRecentOrdersAsync()
    {
        var recentOrders = await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        if (!recentOrders.Any())
            return RedirectToPage();

        var items = recentOrders.SelectMany(o => o.Items).ToList();
        _db.OrderItems.RemoveRange(items);
        _db.Orders.RemoveRange(recentOrders);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateSellerNoteAsync(string orderNumber, string sellerNote)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return RedirectToPage();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        if (order == null)
            return RedirectToPage();

        order.SellerNote = string.IsNullOrWhiteSpace(sellerNote) ? null : sellerNote.Trim();
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}

public record ItemStat(string Name, int Quantity, decimal Revenue);
public record OrderLine(string Name, int Quantity);
public record OrderSummary(string OrderNumber, DateTime CreatedAtUtc, DateTime PickupTimeUtc, decimal Total, List<OrderLine> Items, string SellerNote);
