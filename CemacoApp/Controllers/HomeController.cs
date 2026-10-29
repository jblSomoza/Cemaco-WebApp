using Microsoft.AspNetCore.Mvc;
namespace CemacoApp.Controllers;
public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
    public IActionResult NoEncontrado() { Response.StatusCode = StatusCodes.Status404NotFound; return View("NotFound"); }
}

