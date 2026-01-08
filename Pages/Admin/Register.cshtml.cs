using System.Security.Claims;
using LocalBakery.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LocalBakery.Pages.Admin;

public class RegisterModel : PageModel
{
    private readonly UserService _users;
    public RegisterModel(UserService users) => _users = users;

    [BindProperty]
    public string UserName { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public bool CanRegister { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        CanRegister = !await _users.AnyUsersAsync();
        if (!CanRegister && !User.IsInRole("Admin"))
            return Forbid();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CanRegister = !await _users.AnyUsersAsync();
        if (!CanRegister && !User.IsInRole("Admin"))
            return Forbid();

        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(string.Empty, "Username and password are required.");
            return Page();
        }

        if (await _users.UserExistsAsync(UserName))
        {
            ModelState.AddModelError(string.Empty, "That username is already taken.");
            return Page();
        }

        var user = await _users.CreateUserAsync(UserName, Password, "Admin");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Admin/Listings/Index");
    }
}
