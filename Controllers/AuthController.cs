using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OctoStore.Models;

namespace OctoStore.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly AppSettings _appSettings;

    public AuthController(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        if (string.IsNullOrEmpty(_appSettings.GitHubClientId) || string.IsNullOrEmpty(_appSettings.GitHubClientSecret))
        {
            return View("OAuthNotConfigured");
        }

        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/appsubmission/dashboard"
        }, "GitHub");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return Redirect("/");
    }

    [HttpGet("github-callback")]
    public async Task<IActionResult> GitHubCallback()
    {
        var result = await HttpContext.AuthenticateAsync("GitHub");
        if (!result.Succeeded)
        {
            return BadRequest();
        }

        await HttpContext.SignInAsync("Cookies", result.Principal!);
        return Redirect("/appsubmission/dashboard");
    }
}