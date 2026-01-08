namespace LocalBakery.Models;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N")[..10].ToUpper();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime PickupTimeUtc { get; set; } = DateTime.UtcNow.AddMinutes(30);

    public string? CustomerUserName { get; set; }

    public string Status { get; set; } = "New";

    public string? SellerNote { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
