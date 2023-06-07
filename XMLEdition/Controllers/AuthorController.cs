using Microsoft.AspNetCore.Mvc;
using XMLEdition.Data.Repositories.Repositories;

namespace XMLEdition.Controllers
{
    [Route("/Author")]
    public class AuthorController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private CourseRepository _courseRepository;

        public AuthorController(Data.AppContext context)
        {
            _context = context;
            _courseRepository = new CourseRepository(context);
        }

        [Route("/Author/Index/{userId}")]
        public IActionResult Index(Guid userId)
        {
            ViewBag.Courses = _courseRepository.GetAllAuthorsCourses(userId);

            return View();
        }

        [Route("/Author/CreateCourse")]
        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}
