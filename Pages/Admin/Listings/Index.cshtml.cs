using LocalBakery.Data;
using LocalBakery.Models;
using Microsoft.AspNetCore.Mvc;
using LocalBakery.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Admin.Listings;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<MenuItem> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _db.MenuItems
            .AsNoTracking()
            .OrderByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRefreshInventoryAsync()
    {
        var today = TimeHelper.GetSgtDate();
        var items = await _db.MenuItems.ToListAsync();
        foreach (var item in items)
        {
            if (item.DailyStock <= 0)
            {
                item.DailyStockRemaining = 0;
                item.StockResetDateUtc = today;
                continue;
            }

            item.DailyStockRemaining = item.DailyStock;
            item.StockResetDateUtc = today;
        }

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
