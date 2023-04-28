using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using XMLEdition.Data;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Data.AppContext _context = new Data.AppContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Courses = _context.Courses.ToList();

            return View();
        }

        public IActionResult Video()
        {
            return View();
        }

        [Route("/Home/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            ViewBag.Lesson = _context.Lessons.Where(lesson => lesson.Id == id).FirstOrDefault();

            return View();
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