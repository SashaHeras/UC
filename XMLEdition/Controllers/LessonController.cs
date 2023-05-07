using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.WindowsAzure.Storage;
using XMLEdition.Data;
using XMLEdition.Data.Repositories.Repositories;
using XMLEdition.Migrations;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class LessonController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private LessonRepository _lessonRepository;
        private CourseItemRepository _courseItemRepository;

        public LessonController(Data.AppContext context)
        {
            _context = context;
            _lessonRepository = new LessonRepository(context);
            _courseItemRepository = new CourseItemRepository(context);
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

        public async Task<bool> SaveVideoAsync(string newname, IFormCollection form, CreateLessonModel c)
        {
            string uploads = "C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\";
            string newName = Guid.NewGuid().ToString().Replace("-", "") + "." + c.VideoPath.FileName.Split(".").Last();

            string filePath = Path.Combine(uploads, c.VideoPath.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await c.VideoPath.CopyToAsync(fileStream);
            }

            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("test");

                await container.CreateIfNotExistsAsync();

                var blockBlob = container.GetBlockBlobReference(newname);

                await using (var stream = System.IO.File.OpenRead("C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\" + c.VideoPath.FileName))
                {
                    await blockBlob.UploadFromStreamAsync(stream);
                }

                return true;
            }
            catch (Exception ex)
            {
                // handle exceptions
                return false;
            }
        }

        [HttpPost]
        public IActionResult Create(IFormCollection form, CreateLessonModel c)
        {
            Guid newFileName = Guid.NewGuid();
            SaveVideoAsync(newFileName.ToString(), form, c);

            var sameCourseItems = _courseItemRepository.GetCourseItemsByCourseId(Convert.ToInt32(c.CourseId)).OrderBy(ci => ci.OrderNumber);

            CourseItem newCourceItem = new CourseItem()
            {
                TypeId = _context.CourseItemTypes.Where(cit => cit.Name == "Lesson").FirstOrDefault().Id,
                CourseId = Convert.ToInt32(c.CourseId),
                DateCreation = DateTime.Now,
                OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1,
                StatusId = 2
            };

            _context.CourseItem.Add(newCourceItem);
            _context.SaveChanges();

            Lesson newLesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Theme = c.Theme,
                Description = c.Description,
                Body = c.Body,
                VideoPath = newFileName.ToString(),
                CourseItemId = newCourceItem.Id,
                DateCreation = DateTime.Now.ToShortDateString()
            };

            _lessonRepository.AddAsync(newLesson);

            return RedirectToAction("CreateCourse", "Course", new { id = newCourceItem.CourseId });
        }

        [Route("/Lesson/GoToLesson/{id}")]
        public IActionResult GoToLesson(int id)
        {
            Guid lessonGuid = _lessonRepository.GetLessonByCourseItemId(id).Id;

            return RedirectToAction("Lesson", new { id = lessonGuid });
        }

        [Route("/Lesson/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            var lesson = _lessonRepository.GetLessonById(id);
            var courseItemId = lesson.CourseItemId;
            var courseId = _context.CourseItem.Where(c => c.Id == courseItemId).FirstOrDefault().CourseId;

            ViewBag.Lesson = lesson;
            ViewBag.Course = _context.Courses.Where(c => c.Id == courseId).FirstOrDefault();

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

            int courseId = currentCourseItem.CourseId;

            _courseItemRepository.UpdateAsync(currentCourseItem);

            Lesson currentLesson = _lessonRepository.GetLessonById(lesson.Id);

            currentLesson.Theme = lesson.Theme;
            currentLesson.Description = lesson.Description;
            currentLesson.Body = lesson.Body;
            currentLesson.CourseItemId = lesson.CourseItemId;
            currentLesson.DateCreation = DateTime.Now.ToShortDateString();

            _lessonRepository.UpdateAsync(currentLesson);

            return RedirectToAction("CreateCourse", "Course", new { id = courseId });
        }

        public async Task saveInAzure(string filename)
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mystudystorage;AccountKey=F2DhOdWx3qBaoImpVaDkLDVCyErlLVghvKL5kcxYLL9V7KsOQobaH8wWSh4m48ACDDK/lnsyzd3Q+AStciFc5Q==;EndpointSuffix=core.windows.net";

            // Create a BlobServiceClient object using the connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to the container you created
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("test");

            // Get a reference to the blob you want to upload
            BlobClient blobClient = containerClient.GetBlobClient(filename);

            // Upload the video file
            await using(FileStream fileStream = System.IO.File.OpenRead("C:\\Users\\acsel\\source\\repos\\XMLEdition\\XMLEdition\\wwwroot\\Videos\\" + filename))
            {
                await blobClient.UploadAsync(fileStream, true);
            }
        }
    }
}
