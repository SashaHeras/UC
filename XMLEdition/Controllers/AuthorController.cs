using Microsoft.AspNetCore.Mvc;

namespace XMLEdition.Controllers
{
    public class AuthorController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();


        [Route("/Author/Index/{userId}")]
        public IActionResult Index(Guid userId)
        {
            // {5cc5918d-81b7-4a84-bd64-e79fd914ebf7}

            var courses = _context.Courses.Where(c=>c.AuthorId == userId).ToList();
            ViewBag.Courses = courses;

            return View();
        }

        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}
