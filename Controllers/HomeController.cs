using Microsoft.AspNetCore.Mvc;

namespace OctoStore.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/appsubmission/dashboard");
        }
        
        return View();
    }
}