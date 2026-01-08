using LocalBakery.Data;
using LocalBakery.Models;
using LocalBakery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Admin.Listings;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;
    public EditModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    [BindProperty]
    public MenuItem Item { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (item == null)
            return RedirectToPage("/Admin/Listings/Index");

        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        if (Item.Id == 0 && id != 0)
            Item.Id = id;

        var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == Item.Id);
        if (item == null)
            return RedirectToPage("/Admin/Listings/Index");

        item.Name = Item.Name;
        item.Description = Item.Description;
        item.Price = Item.Price;
        item.Category = Item.Category;
        item.TagsCsv = Item.TagsCsv;
        item.Allergens = Item.Allergens;
        item.IsAvailable = Item.IsAvailable;
        var today = DateTime.UtcNow.Date;
        var previousDailyStock = item.DailyStock;
        item.DailyStock = Item.DailyStock;
        if (item.DailyStock <= 0)
        {
            item.DailyStockRemaining = 0;
            item.StockResetDateUtc = today;
        }
        else if (item.DailyStock != previousDailyStock)
        {
            item.DailyStockRemaining = item.DailyStock;
            item.StockResetDateUtc = today;
        }

        try
        {
            var uploadedFile = ImageFile ?? Request.Form.Files.FirstOrDefault();
            var imagePath = await _images.SaveImageAsync(uploadedFile);
            if (!string.IsNullOrWhiteSpace(imagePath))
                item.ImagePath = imagePath;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        await _db.SaveChangesAsync();

        return RedirectToPage("/Admin/Listings/Index");
    }
}
