using LocalBakery.Data;
using LocalBakery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocalBakery.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> AnyUsersAsync()
    {
        return await _db.AppUsers.AnyAsync();
    }

    public async Task<AppUser> CreateUserAsync(string userName, string password, string role)
    {
        var normalized = userName.Trim().ToUpperInvariant();
        var user = new AppUser
        {
            UserName = userName.Trim(),
            NormalizedUserName = normalized,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UserExistsAsync(string userName)
    {
        var normalized = userName.Trim().ToUpperInvariant();
        return await _db.AppUsers.AnyAsync(u => u.NormalizedUserName == normalized);
    }

    public async Task<AppUser?> ValidateCredentialsAsync(string userName, string password)
    {
        var normalized = userName.Trim().ToUpperInvariant();
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
        if (user == null)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }
}
