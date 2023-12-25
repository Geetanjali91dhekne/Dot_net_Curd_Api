using Microsoft.AspNetCore.Mvc;

namespace WebAPIs.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
