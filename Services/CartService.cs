using System.Text.Json;

namespace LocalBakery.Services;

public class CartService
{
    private const string SessionKey = "rewear-cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CartItem> GetItems() => GetCartItems();

    public int GetItemCount() => GetCartItems().Sum(i => i.Quantity);

    public decimal GetTotal() => GetCartItems().Sum(i => i.Price * i.Quantity);

    public void AddItem(CartItem item, int quantity)
    {
        var items = GetCartItems();
        var existing = items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            item.Quantity = quantity;
            items.Add(item);
        }

        SaveCartItems(items);
    }

    public void UpdateQuantity(int menuItemId, int quantity)
    {
        var items = GetCartItems();
        var existing = items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existing == null)
            return;

        if (quantity <= 0)
            items.Remove(existing);
        else
            existing.Quantity = quantity;

        SaveCartItems(items);
    }

    public void RemoveItem(int menuItemId)
    {
        var items = GetCartItems();
        items.RemoveAll(i => i.MenuItemId == menuItemId);
        SaveCartItems(items);
    }

    public void Clear()
    {
        SaveCartItems(new List<CartItem>());
    }

    private List<CartItem> GetCartItems()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return new List<CartItem>();

        var payload = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(payload))
            return new List<CartItem>();

        return JsonSerializer.Deserialize<List<CartItem>>(payload) ?? new List<CartItem>();
    }

    private void SaveCartItems(List<CartItem> items)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;

        session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }
}

public class CartItem
{
    public int MenuItemId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string ImagePath { get; set; } = "";
    public int Quantity { get; set; } = 1;
}
