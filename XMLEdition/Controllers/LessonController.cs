using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.WindowsAzure.Storage;
using XMLEdition.Core.Services;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;
using XMLEdition.Migrations;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class LessonController : Controller
    {
        private ProjectContext _context = new ProjectContext();
        private LessonService _lessonService;
        private AzureService _azureService;

        public LessonController(ProjectContext context)
        {
            _context = context;
            _lessonService = new LessonService(context);
            _azureService = new AzureService();
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
        public IActionResult Create(IFormCollection form, CreateLessonModel createLessonModel)
        {
            string videoName = _azureService.SaveInAsync(createLessonModel.VideoPath).Result;
            CourseItem newCourceItem = _lessonService.CreateNewCourseItem(createLessonModel);
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
            var courseId = _lessonService.GetCourseItem(lesson.CourseItemId).CourseId;

            ViewBag.Lesson = lesson;
            ViewBag.Course = _lessonService.GetCourse(courseId);

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

            CourseItem currentCourseItem = _lessonService.GetCourseItem(lesson.CourseItemId);
            currentCourseItem.DateCreation = DateTime.Now;

            int courseId = currentCourseItem.CourseId;
            await _lessonService.UpdateCourseItem(currentCourseItem);   
            _lessonService.UpdateLesson(lesson);

            _azureService.DeleteMediaFromProject(form.Files[0]);

            return RedirectToAction("CreateCourse", "Course", new { id = courseId });
        }
    }
}
