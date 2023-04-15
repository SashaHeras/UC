using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using XMLEdition.Data;

namespace XMLEdition.Controllers
{
    public class LessonController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();

        public IActionResult Index()
        {
            return View();
        }


        [Route("/Lesson/EditLesson/{id}")]
        public IActionResult EditLesson(int id)
        {
            ViewBag.Lesson = _context.Lessons.Where(l => l.CourseItemId == id).FirstOrDefault();

            return View();  
        }

        [Route("/Lesson/Edit")]
        public IActionResult Edit(Lesson lesson)
        {
            int courseId = _context.CourseItem.Where(ci => ci.Id == lesson.CourseItemId).FirstOrDefault().CourseId;

            return View();
        }
    }
}
