using LocalBakery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LocalBakery.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly CartService _cart;
    public IndexModel(CartService cart) => _cart = cart;

    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }

    public void OnGet()
    {
        Items = _cart.GetItems();
        Total = _cart.GetTotal();
    }

    public IActionResult OnPostUpdate(int id, int quantity)
    {
        _cart.UpdateQuantity(id, quantity);
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int id)
    {
        _cart.RemoveItem(id);
        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        _cart.Clear();
        return RedirectToPage();
    }
}
