using Microsoft.AspNetCore.Mvc;
using XMLEdition.Core.Services;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Controllers
{
    public class AuthorController : Controller
    {
        private CourseService _courseService;

        public AuthorController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [Route("/Author/Index/{userId}")]
        public IActionResult Index(Guid userId)
        {
            ViewBag.Courses = _courseService.GetAuthorsCourses(userId);

            return View();
        }

        [Route("/Author/CreateCourse")]
        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}
