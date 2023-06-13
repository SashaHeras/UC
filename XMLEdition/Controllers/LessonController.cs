using Microsoft.AspNetCore.Mvc;
using XMLEdition.Core.Services;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.ViewModels;

namespace XMLEdition.Controllers
{
    public class LessonController : Controller
    {
        private LessonService _lessonService;
        private AzureService _azureService;
        private MediaService _mediaService;
        private CourseItemService _courseItemService;
        private CourseService _courseService;

        public LessonController(LessonService lessonService, AzureService azureService, MediaService mediaService, 
            CourseItemService courseItemService, CourseService courseService)
        {
            _lessonService = lessonService;
            _azureService = azureService;
            _mediaService = mediaService;
            _courseItemService = courseItemService;
            _courseService = courseService;
        }

        /// <summary>
        /// Main page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Page of lesson creation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("/Lesson/CreateLesson/{id}")]
        public IActionResult CreateLesson(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        /// <summary>
        /// Method of lesson creation
        /// </summary>
        /// <param name="form"></param>
        /// <param name="createLessonModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection form, CreateLessonModel createLessonModel)
        {
            string videoName = await _azureService.SaveInAsync(createLessonModel.VideoPath);
            CourseItem newCourceItem = await _courseItemService.CreateNewCourseItem(createLessonModel);
            _lessonService.CreateLesson(newCourceItem, createLessonModel, videoName);

            return RedirectToAction("CreateCourse", "Course", new { id = newCourceItem.CourseId });
        }

        [Route("/Lesson/GoToLesson/{id}")]
        public IActionResult GoToLesson(int id)
        {
            Guid lessonGuid = _lessonService.GetLessonByCourseItem(id).Id;
            return RedirectToAction("Lesson", new { id = lessonGuid });
        }

        [Route("/Lesson/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            var lesson = _lessonService.GetLesson(id);
            var courseId = _courseItemService.GetCourseItem(lesson.CourseItemId).CourseId;

            ViewBag.Lesson = lesson;
            ViewBag.Course = _courseService.GetCourse(courseId);

            return View();
        }

        [Route("/Lesson/EditLesson/{id}")]
        public IActionResult EditLesson(int id)
        {
            Lesson l = _lessonService.GetLessonByCourseItem(id);
            ViewBag.Lesson = l;

            return View(l);  
        }

        [Route("/Lesson/Edit")]
        public async Task<IActionResult> Edit(IFormCollection form, Lesson lesson)
        {
            string videoPath = String.Empty;
            string oldVideoName = String.Empty;

            if (form.Files.Count != 0)
            {
                await _azureService.DeleteFromAzure(lesson.VideoPath);
                videoPath = _azureService.SaveInAsync(form.Files[0]).Result;
                lesson.VideoPath = videoPath;
            }
            else
            {
                oldVideoName = _lessonService.GetLesson(lesson.Id).VideoPath;
                lesson.VideoPath = oldVideoName;
            }

            CourseItem currentCourseItem = _courseItemService.GetCourseItem(lesson.CourseItemId);
            currentCourseItem.DateCreation = DateTime.Now;

            int courseId = currentCourseItem.CourseId;
            await _courseItemService.UpdateCourseItem(currentCourseItem);  
            
            _lessonService.UpdateLesson(lesson);

            _mediaService.DeleteMediaFromProject(form.Files[0]);

            return RedirectToAction("CreateCourse", "Course", new { id = courseId });
        }
    }
}
