using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using XMLEdition.Models;
using XMLEdition.Core.Services;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;

namespace XMLEdition.Controllers
{
    public class CourseController : Controller
    {
        private ProjectContext _context = new ProjectContext();
        private CourseRepository _courseRepository;
        private CourseItemRepository _courseItemRepository;
        private CourseTypeRepository _courseTypeRepository;
        private LessonRepository _lessonRepository;
        private CourseService _courseService;

        public CourseController(CourseService service)
        {
            //_context = context;
            //_courseRepository = new CourseRepository(context);
            //_courseItemRepository = new CourseItemRepository(context);
            //_courseTypeRepository = new CourseTypeRepository(context);
            //_lessonRepository = new LessonRepository(context);
            _courseService = service;    
        }

        public IActionResult Index(int id)
        {
            List<CourseElement> elements = _courseService.GetCourseElements(id);
            ViewBag.CourseId = id;            
            ViewBag.DefaultTypeId = elements[0].TypeId; 
            ViewBag.DefaultCourseItemId = elements[0].CourseItemId;

            return View();
        }

        [Route("/Course/CreateCourse/{id}")]
        public IActionResult CreateCourse(int id)
        {
            ViewBag.Subjects = _context.CourseSubjects.ToList();
            if (id == 0)
            {
                ViewBag.Course = _courseService.CreateNewCourse();
            }
            else
            {
                ViewBag.Course = _courseService._courseRepository.GetCourse(id);
            }            
            return View();
        }

        [HttpGet]
        public JsonResult GetElements()
        {
            var elements = _courseService.GetElementsByCourseId(Convert.ToInt32(Request.Form["id"]));
            return Json(elements);
        }

        [HttpGet]
        public JsonResult GetElementName()
        {
            int courseItemId = Convert.ToInt32(Request.Form["elementId"]);
            var element = _courseItemRepository.GetCourseItemById(courseItemId);
            var typeName = _courseTypeRepository.GetTypeById(element.TypeId).Name;

            return Json(element, typeName);
        }

        [HttpPost]
        public JsonResult GetCourseElements()
        {
            var result = _courseService.GetCourseElements(Convert.ToInt32(Request.Form["course"]));
            return Json(result);
        }

        [HttpPost]
        public JsonResult SaveCource()
        {
            var form = Request.Form;
            IFormFile? picture = null;
            IFormFile? video = null;
            if (form.Files.Count != 0)
            {
                video = form.Files["video"];
                picture = form.Files["picture"];
            }

            int courseId = Convert.ToInt32(form["courseId"]);
            Course newCourse = new Course();

            newCourse.Name = form["name"].ToString();
            newCourse.AuthorId = Guid.Parse(form["authorId"].ToString());
            newCourse.Checked = false;
            newCourse.Price = Convert.ToDecimal(form["price"].ToString());
            newCourse.CourseSubjectId = Convert.ToInt32(form["subject"]);
            newCourse.LastEdittingDate = DateTime.Now;

            newCourse.PicturePath = _courseService.SavePicture(picture, courseId);
            newCourse.PreviewVideoPath = _courseService.SaveVideo(video, courseId);
            
            if (courseId == 0)
            {
                _courseRepository.AddAsync(newCourse);
            }
            else
            {
                newCourse.Id = courseId;
                _courseRepository.UpdateAsync(newCourse);
            }

            return Json(newCourse.Id);
        }

        [HttpDelete]
        [Route("/Course/DeleteCourseItem/{courseItemId}/{typeId}")]
        public JsonResult DeleteCourseItem(int courseItemId, int typeId)
        {
            var item = _courseItemRepository.GetCourseItemById(courseItemId);

            try
            {
                item.StatusId = _context.ItemsStatuses.Where(i => i.Name == "Deleted").FirstOrDefault().Id;
                _courseItemRepository.UpdateAsync(item);
            }
            catch(Exception ex)
            {
                return Json(ex.Message);
            }

            return Json(true);
        }

        [HttpGet]
        [Route("/Course/Comments/{courseId}/{commentsCount}")]
        public JsonResult GetComments(int courseId, int commentsCount = 10)
        {
            List<CommentModel> commentsList = new List<CommentModel>();
            var comments = _context.Comments.Where(c => c.CourseId == courseId);
            foreach (var comment in comments)
            {
                commentsList.Add(new CommentModel()
                {
                    Id = comment.Id,
                    Text = comment.Text,
                    Rating = comment.Rating,
                    DateAgo = GetTimeSinceDate(comment.DateCreation),
                    UserName = "",              // Add user name Heras O.
                    ProfileImagePath = ""       // Add user profile image name
                });
            }

            commentsList.Add(new CommentModel()
            {
                Id = 5,
                Text = "Hello",
                Rating = 4,
                DateAgo = "4 month ago",
                UserName = "Heras A.",              // Add user name Heras O.
                ProfileImagePath = null       // Add user profile image name
            });

            return Json(commentsList);
        }

        [HttpPost]
        public IActionResult LoadPartialPage(string type, string courseItemId)
        {
            if (type == "1")
            {

            }

            int itemId = Convert.ToInt32(courseItemId);
            int courseId = _context.CourseItem.Where(ci => ci.Id == itemId).FirstOrDefault().CourseId;

            Course c = _courseRepository.GetCourse(courseId);
            Lesson l = _lessonRepository.GetLessonByCourseItemId(itemId);

            LessonPartial lessonPartial = new LessonPartial()
            {
                Id = l.Id,
                Theme = l.Theme,
                Description = l.Description,
                VideoPath = l.VideoPath,
                Body = l.Body,
                DateCreation = l.DateCreation,
                CourseItemId = l.CourseItemId,
                CourseId = c.Id,
                Rating = c.Rating
            };

            return PartialView("~/Views/Partial/_LessonPartial.cshtml", lessonPartial);
        }

        public async Task<string> SaveInAsync(IFormFile file)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Pictures\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "");

            string filePath = Path.Combine(uploads, file.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("test");

                await container.CreateIfNotExistsAsync();

                var blockBlob = container.GetBlockBlobReference(newName);

                await using (var stream = System.IO.File.OpenRead(uploads + file.FileName))
                {
                    await blockBlob.UploadFromStreamAsync(stream);
                }

                return newName;
            }
            catch (Exception ex)
            {
                // handle exceptions
                return "";
            }
        }

        public async Task<bool> DeleteFromAzure(string name)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("test");
                var blockBlob = container.GetBlockBlobReference(name);

                if (await blockBlob.DeleteIfExistsAsync())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static string GetTimeSinceDate(DateTime date)
        {
            TimeSpan timeSpan = DateTime.Now - date;

            int totalDays = (int)timeSpan.TotalDays;
            int totalWeeks = totalDays / 7;
            int totalMonths = totalDays / 30;
            int totalYears = totalDays / 365;

            if (totalWeeks < 4)
            {
                return $"{totalWeeks} week{(totalWeeks == 1 ? "" : "s")} ago";
            }
            else if (totalMonths < 12)
            {
                return $"{totalMonths} month{(totalMonths == 1 ? "" : "s")} ago";
            }
            else
            {
                return $"{totalYears} year{(totalYears == 1 ? "" : "s")} ago";
            }
        }
    }
}
