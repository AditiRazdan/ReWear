using System.ComponentModel.DataAnnotations;

namespace LocalBakery.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";

    [Range(0, 9999)]
    public decimal Price { get; set; }

    [MaxLength(40)]
    public string Category { get; set; } = "General";

    [MaxLength(120)]
    public string TagsCsv { get; set; } = "";

    [MaxLength(200)]
    public string Allergens { get; set; } = "";

    [MaxLength(300)]
    public string ImagePath { get; set; } = "";

    public int DailyStock { get; set; } = 0;

    public int DailyStockRemaining { get; set; } = 0;

    public DateTime? StockResetDateUtc { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
