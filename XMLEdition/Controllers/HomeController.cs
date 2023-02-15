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
            Serializer.Read();
            ViewBag.Products = Serializer.products.OrderBy(p => p.Id);

            return View();
        }

        public IActionResult DeleteProduct(int id)
        {
            var prod = Serializer.products.Where(p => p.Id == id).FirstOrDefault();
            Serializer.Delete(prod);

            return RedirectToAction("Index");
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult AddProduct(Product p)
        {
            p.Id = Serializer.products.OrderBy(p => p.Id).Last().Id + 1;
            Serializer.Add(p);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Product = Serializer.products.Where(p=>p.Id == id).FirstOrDefault();

            return View();
        }

        public IActionResult EditProduct(Product p)
        {
            Serializer.Update(p);

            return RedirectToAction("Index");
        }

        public IActionResult Video()
        {
            return View();
        }

        public IActionResult Lesson()
        {
            ViewBag.Lesson = _context.Lessons.FirstOrDefault();

            return View();
        }

        public IActionResult CreateLesson()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateLessonModel c)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "") + "." + c.VideoPath.FileName.Split(".").Last();

            string filePath = Path.Combine(uploads, c.VideoPath.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                c.VideoPath.CopyToAsync(fileStream);
            }

            //FileInfo file = new FileInfo(filePath);
            //file.MoveTo("C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\" + newName);

            Lesson l = new Lesson()
            {
                Id = Guid.NewGuid(),
                Theme = c.Theme,
                Description = c.Description,
                Body = c.Body,
                VideoPath = c.VideoPath.FileName,
                DateCreation = DateTime.Now.ToShortDateString()
            };

            _context.Lessons.Add(l);
            _context.SaveChanges();

            return RedirectToAction("Index");
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