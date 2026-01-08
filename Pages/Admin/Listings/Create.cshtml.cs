using LocalBakery.Data;
using LocalBakery.Models;
using LocalBakery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LocalBakery.Pages.Admin.Listings;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;
    public CreateModel(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    [BindProperty]
    public MenuItem Item { get; set; } = new() { IsAvailable = true };

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var uploadedFile = ImageFile ?? Request.Form.Files.FirstOrDefault();
            var imagePath = await _images.SaveImageAsync(uploadedFile);
            if (!string.IsNullOrWhiteSpace(imagePath))
                Item.ImagePath = imagePath;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        Item.CreatedAtUtc = DateTime.UtcNow;
        var today = DateTime.UtcNow.Date;
        Item.StockResetDateUtc = today;
        if (Item.DailyStock > 0)
            Item.DailyStockRemaining = Item.DailyStock;
        else
            Item.DailyStockRemaining = 0;
        _db.MenuItems.Add(Item);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Admin/Listings/Index");
    }
}
