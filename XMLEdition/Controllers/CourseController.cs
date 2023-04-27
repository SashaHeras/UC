using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using XMLEdition.Data;
using XMLEdition.Data.Repositories.Repositories;

namespace XMLEdition.Controllers
{
    public class CourseController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private CourseRepository _courseRepository;

        public CourseController(Data.AppContext context)
        {
            _context = context;
            _courseRepository = new CourseRepository(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Course/CreateCourse/{id}")]
        public IActionResult CreateCourse(int id)
        {
            if(id == 0)
            {
                ViewBag.Course = new Course()
                {
                    Id = 0,
                    Name = "",
                    Price = 0
                };

                return View();
            }

            ViewBag.Subjects = _context.CourseSubjects.ToList();
            ViewBag.Course = _courseRepository.GetCourse(id);

            return View();
        }

        [HttpGet]
        public JsonResult GetElements()
        {
            var elements = _context.CourseItem.Where(ci=>ci.CourseId == Convert.ToInt32(Request.Form["id"]));

            return Json(elements);
        }

        [HttpGet]
        public JsonResult GetElementName()
        {
            var element = _context.CourseItem.Where(e => e.Id == Convert.ToInt32(Request.Form["elementId"])).FirstOrDefault();
            var typeName = _context.CourseItemTypes.Where(t => t.Id == element.TypeId).FirstOrDefault().Name;

            return Json(element, typeName);
        }

        [HttpPost]
        public JsonResult GetCourseElements()
        {
            var result = _courseRepository.GetCourseElementsList(Request.Form["course"].ToString());
            return Json(result);
        }

        [HttpPost]
        public JsonResult SaveCource()
        {
            Course newCourse = new Course();
            var form = Request.Form;
            IFormFile file = null;
            if (form.Files.Count != 0)
            {
                file = form.Files[0];
            }
            string filePath = "";
            
            if (form["courseId"].ToString() == "0")
            {
                newCourse = new Course()
                {
                    Name = form["name"].ToString(),
                    AuthorId = Guid.Parse(form["authorId"].ToString()),
                    Checked = false,
                    Price = Convert.ToDecimal(form["price"].ToString()),
                    CourseSubjectId = Convert.ToInt32(form["subject"]),
                    LastEdittingDate = DateTime.Now
                };

                if (file != null)
                {
                    newCourse.PicturePath = SavePicture(file);
                }

                _courseRepository.AddAsync(newCourse);
            }
            else
            {
                newCourse = _courseRepository.GetCourse(Convert.ToInt32(Request.Form["courseId"].ToString()));
                int courseId = newCourse.Id;
                string path = newCourse.PicturePath;

                _context.Courses.Remove(newCourse);

                newCourse = new Course()
                {
                    Id = courseId,
                    Name = form["name"].ToString(),
                    AuthorId = Guid.Parse(form["authorId"].ToString()),
                    Checked = false,
                    Price = Convert.ToDecimal(form["price"].ToString()),
                    CourseSubjectId = Convert.ToInt32(form["subject"]),
                    LastEdittingDate = DateTime.Now
                };

                if (file != null)
                {
                    newCourse.PicturePath = SavePicture(file);
                }
                else
                {
                    newCourse.PicturePath = path;
                }

                _courseRepository.AddAsync(newCourse);
            }

            return Json(newCourse.Id);
        }

        public string SavePicture(IFormFile file)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Pictures\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "") + "." + file.FileName.Split(".").Last();

            string filePath = Path.Combine(uploads, file.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyToAsync(fileStream);
            }

            return file.FileName;
        }
    }
}
