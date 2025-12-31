using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PayGoHub.Web.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback", new { returnUrl })
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToAction("Login");
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult GetUserInfo()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Json(new { authenticated = false });
        }

        var claims = User.Claims.ToList();
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        var surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value
                   ?? claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

        return Json(new
        {
            authenticated = true,
            email,
            name,
            givenName,
            surname,
            picture
        });
    }
}
