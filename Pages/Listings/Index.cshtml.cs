using LocalBakery.Data;
using LocalBakery.Models;
using LocalBakery.Services;
using LocalBakery.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Listings;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;
    public IndexModel(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public List<MenuItem> Items { get; set; } = new();

    public string? Q { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string HoursLabel { get; set; } = "";

    public async Task OnGetAsync(string? q, string? category, string? tag)
    {
        Q = q;
        Category = category;
        Tag = tag;
        HoursLabel = "Daily 8:00 AM - 8:00 PM (SGT)";

        var items = await _db.MenuItems
            .Where(m => m.IsAvailable)
            .ToListAsync();

        var today = TimeHelper.GetSgtDate();
        var needsSave = false;
        foreach (var item in items)
        {
            if (item.DailyStock <= 0)
                continue;

            var lastReset = item.StockResetDateUtc?.Date;
            if (lastReset == null || lastReset.Value < today)
            {
                item.DailyStockRemaining = item.DailyStock;
                item.StockResetDateUtc = today;
                needsSave = true;
            }
        }

        if (needsSave)
            await _db.SaveChangesAsync();

        items = items
            .Where(m => m.DailyStock <= 0 || m.DailyStockRemaining > 0)
            .ToList();

        var normalizedQuery = NormalizeFilter(q);
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            items = items
                .Where(m =>
                    NormalizeFilter(m.Name).Contains(normalizedQuery) ||
                    NormalizeFilter(m.Description).Contains(normalizedQuery))
                .ToList();
        }

        var normalizedCategory = NormalizeFilter(category);
        if (!string.IsNullOrWhiteSpace(normalizedCategory) && normalizedCategory != "all")
        {
            items = items
                .Where(m => NormalizeFilter(m.Category) == normalizedCategory)
                .ToList();
        }

        var normalizedTag = NormalizeFilter(tag);
        if (!string.IsNullOrWhiteSpace(normalizedTag) && normalizedTag != "all")
        {
            items = items.Where(m =>
                    SplitTags(m.TagsCsv)
                        .Any(t => NormalizeFilter(t) == normalizedTag))
                .ToList();
        }

        Items = items;
    }

    private static IEnumerable<string> SplitTags(string? tagsCsv)
    {
        if (string.IsNullOrWhiteSpace(tagsCsv))
            return Array.Empty<string>();

        return tagsCsv
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0);
    }

    private static string NormalizeFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;
        foreach (var ch in value)
        {
            if (ch == '-' || char.IsWhiteSpace(ch))
                continue;

            buffer[index++] = char.ToLowerInvariant(ch);
        }

        return new string(buffer[..index]);
    }

    public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity, string? q, string? category, string? tag)
    {
        var item = await _db.MenuItems.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);

        if (item != null)
        {
            var requested = Math.Max(1, quantity);
            if (item.DailyStock > 0)
                requested = Math.Min(requested, item.DailyStockRemaining);
            if (requested <= 0)
                return RedirectToPage(new { q, category, tag });

            _cart.AddItem(new CartItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                Price = item.Price,
                ImagePath = item.ImagePath
            }, requested);
        }

        return RedirectToPage(new { q, category, tag });
    }
}
