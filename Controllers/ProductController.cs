using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
