using LocalBakery.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Account;

public class ReceiptModel : PageModel
{
    private readonly AppDbContext _db;
    public ReceiptModel(AppDbContext db) => _db = db;

    public string OrderNumber { get; set; } = "";
    public ReceiptOrder? Order { get; set; }

    public async Task<IActionResult> OnGetAsync(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return RedirectToPage("/Account/Orders");

        OrderNumber = order;
        var userName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
            return RedirectToPage("/Account/Login", new { returnUrl = $"/Account/Receipt?order={order}" });

        var isAdmin = User.IsInRole("Admin");
        var existing = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.OrderNumber == order);

        if (existing == null)
            return Page();

        if (!isAdmin && !string.Equals(existing.CustomerUserName, userName, StringComparison.Ordinal))
            return Forbid();

        Order = new ReceiptOrder(
            existing.OrderNumber,
            existing.CreatedAtUtc,
            existing.PickupTimeUtc,
            existing.Items.Sum(i => i.UnitPrice * i.Quantity),
            existing.Items.Select(i => new ReceiptLine(i.MenuItem.Name, i.Quantity, i.UnitPrice)).ToList(),
            existing.SellerNote ?? string.Empty
        );

        return Page();
    }
}

public record ReceiptLine(string Name, int Quantity, decimal UnitPrice);
public record ReceiptOrder(string OrderNumber, DateTime CreatedAtUtc, DateTime PickupTimeUtc, decimal Total, List<ReceiptLine> Items, string SellerNote);
