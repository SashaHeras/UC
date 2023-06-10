using Microsoft.AspNetCore.Mvc;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Controllers
{
    public class AuthorController : Controller
    {
        private ProjectContext _context = new ProjectContext();
        private CourseRepository _courseRepository;

        public AuthorController(ProjectContext context)
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
