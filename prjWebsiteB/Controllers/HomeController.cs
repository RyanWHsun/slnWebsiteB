using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using prjWebsiteB.Models;

namespace prjWebsiteB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        dbGroupBContext _context;
        public HomeController(ILogger<HomeController> logger, dbGroupBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Test() 
        { 
            //dbGroupBContext context = new dbGroupBContext();
            return View(_context.TUsers);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
