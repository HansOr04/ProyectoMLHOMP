using Microsoft.AspNetCore.Mvc;

namespace ProyectoMLHOMP.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
