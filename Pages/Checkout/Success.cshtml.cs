using LocalBakery.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Checkout;

public class SuccessModel : PageModel
{
    private readonly AppDbContext _db;
    public SuccessModel(AppDbContext db) => _db = db;

    public string OrderNumber { get; set; } = "";
    public DateTime? PickupTimeUtc { get; set; }

    public async Task<IActionResult> OnGetAsync(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return RedirectToPage("/Listings/Index");

        OrderNumber = order;
        var existing = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == order);

        PickupTimeUtc = existing?.PickupTimeUtc;
        return Page();
    }
}
