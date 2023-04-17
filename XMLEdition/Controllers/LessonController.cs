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
            Lesson l = _context.Lessons.Where(l => l.CourseItemId == id).FirstOrDefault();
            ViewBag.Lesson = l;

            return View(l);  
        }

        [Route("/Lesson/Edit")]
        public IActionResult Edit(IFormCollection form, Lesson lesson)
        {
            int courseId = _context.CourseItem.Where(ci => ci.Id == lesson.CourseItemId).FirstOrDefault().CourseId;

            if (form.Files.Count != 0)
            {
                string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\";
                string newName = Guid.NewGuid().ToString().Replace("-", "") + "." + form.Files[0].FileName.Split(".").Last();

                string filePath = Path.Combine(uploads, form.Files[0].FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    form.Files[0].CopyToAsync(fileStream);
                }
            }

            CourseItem currentCourseItem = _context.CourseItem.Where(c => c.Id == lesson.CourseItemId).FirstOrDefault();

            currentCourseItem.DateCreation = DateTime.Now;

            _context.CourseItem.Update(currentCourseItem);
            _context.SaveChanges();

            Lesson currentLesson = _context.Lessons.Where(l=>l.Id == lesson.Id).FirstOrDefault();

            currentLesson.Theme = lesson.Theme;
            currentLesson.Description = lesson.Description;
            currentLesson.Body = lesson.Body;
            currentLesson.CourseItemId = lesson.CourseItemId;
            currentLesson.DateCreation = DateTime.Now.ToShortDateString();

            _context.Lessons.Update(currentLesson);
            _context.SaveChanges();

            return RedirectToAction("CreateCourse", "Course", new { id = courseId });
        }
    }
}
