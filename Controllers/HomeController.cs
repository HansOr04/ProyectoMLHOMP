using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;
using System.Diagnostics;

namespace ProyectoMLHOMP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProyectoContext _context;

        public HomeController(ILogger<HomeController> logger, ProyectoContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var apartments = await _context.Apartment
                    .Include(a => a.Owner)
                    .Where(a => a.IsAvailable)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return View(apartments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los apartamentos");
                return View(new List<Apartment>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}