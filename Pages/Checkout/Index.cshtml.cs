using LocalBakery.Data;
using LocalBakery.Models;
using LocalBakery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Pages.Checkout;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;
    public IndexModel(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }

    public void OnGet()
    {
        Items = _cart.GetItems();
        Total = _cart.GetTotal();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Items = _cart.GetItems();
        Total = _cart.GetTotal();
        if (!Items.Any())
            return RedirectToPage("/Cart/Index");

        var ids = Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _db.MenuItems.Where(m => ids.Contains(m.Id)).ToListAsync();
        var menuItemsById = menuItems.ToDictionary(m => m.Id);

        foreach (var cartItem in Items)
        {
            if (!menuItemsById.TryGetValue(cartItem.MenuItemId, out var menuItem))
                continue;

            if (!menuItem.IsAvailable)
            {
                ModelState.AddModelError(string.Empty, $"{menuItem.Name} is not available.");
                continue;
            }

            if (menuItem.DailyStock > 0 && menuItem.DailyStockRemaining < cartItem.Quantity)
                ModelState.AddModelError(string.Empty, $"Not enough stock for {menuItem.Name}.");
        }

        if (!ModelState.IsValid)
            return Page();

        foreach (var cartItem in Items)
        {
            if (!menuItemsById.TryGetValue(cartItem.MenuItemId, out var menuItem))
                continue;

            if (menuItem.DailyStock > 0)
                menuItem.DailyStockRemaining -= cartItem.Quantity;
        }

        var order = new Order
        {
            CreatedAtUtc = DateTime.UtcNow,
            PickupTimeUtc = DateTime.UtcNow.AddMinutes(30),
            CustomerUserName = User.Identity?.IsAuthenticated == true ? User.Identity?.Name : null,
            Status = "New",
            Items = Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _cart.Clear();

        return RedirectToPage("/Checkout/Success", new { order = order.OrderNumber });
    }
}
