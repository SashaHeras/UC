using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using XMLEdition.Data;
using XMLEdition.Data.Repositories.Repositories;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class LessonController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private LessonRepository _lessonRepository;

        public LessonController(Data.AppContext context)
        {
            _context = context;
            _lessonRepository = new LessonRepository(context);   
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Lesson/CreateLesson/{id}")]
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

            _lessonRepository.AddAsync(newLesson);

            return RedirectToAction("CreateCourse", "Course", new { id = newCourceItem.CourseId });
        }

        [Route("/Lesson/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            ViewBag.Lesson = _lessonRepository.GetLessonById(id);

            return View();
        }

        [Route("/Lesson/EditLesson/{id}")]
        public IActionResult EditLesson(int id)
        {
            Lesson l = _lessonRepository.GetLessonByCourseItemId(id);
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

            Lesson currentLesson = _lessonRepository.GetLessonById(lesson.Id);

            currentLesson.Theme = lesson.Theme;
            currentLesson.Description = lesson.Description;
            currentLesson.Body = lesson.Body;
            currentLesson.CourseItemId = lesson.CourseItemId;
            currentLesson.DateCreation = DateTime.Now.ToShortDateString();

            _lessonRepository.UpdateAsync(currentLesson);

            return RedirectToAction("CreateCourse", "Course", new { id = courseId });
        }
    }
}
