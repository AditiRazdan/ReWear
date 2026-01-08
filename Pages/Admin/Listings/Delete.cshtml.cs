using LocalBakery.Data;
using LocalBakery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Admin.Listings;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    public DeleteModel(AppDbContext db) => _db = db;

    [BindProperty]
    public MenuItem Item { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (item == null)
            return RedirectToPage("/Admin/Listings/Index");

        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == Item.Id);
        if (item == null)
            return RedirectToPage("/Admin/Listings/Index");

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Admin/Listings/Index");
    }
}
