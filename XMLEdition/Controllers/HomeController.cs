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

        [Route("/Home/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            ViewBag.Lesson = _context.Lessons.Where(lesson => lesson.Id == id).FirstOrDefault();

            return View();
        }

        [Route("/Home/CreateLesson/{id}")]
        public IActionResult CreateLesson(int id)
        {
            ViewBag.CourseId = id;

            return View();
        }

        [HttpPost]
        public IActionResult Create(IFormCollection form, CreateLessonModel c)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "") + "." + c.VideoPath.FileName.Split(".").Last();

            string filePath = Path.Combine(uploads, c.VideoPath.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                c.VideoPath.CopyToAsync(fileStream);
            }

            var sameCourseItems = _context.CourseItem.Where(ci => ci.CourseId == Convert.ToInt32(c.CourseId)).OrderBy(ci => ci.OrderNumber);

            CourseItem newCourceItem = new CourseItem()
            {
                TypeId = _context.CourseItemTypes.Where(cit => cit.Name == "Lesson").FirstOrDefault().Id,
                CourseId = Convert.ToInt32(c.CourseId),
                DateCreation = DateTime.Now,
                OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1
            };

            _context.CourseItem.Add(newCourceItem);
            _context.SaveChanges();

            Lesson newLesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Theme = c.Theme,
                Description = c.Description,
                Body = c.Body,
                VideoPath = c.VideoPath.FileName,
                CourseItemId = newCourceItem.Id,
                DateCreation = DateTime.Now.ToShortDateString()
            };

            _context.Lessons.Add(newLesson);
            _context.SaveChanges();

            return RedirectToAction("CreateCourse", "Course", new { id = newCourceItem.CourseId });
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