﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Repositories;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private LessonRepository _lessonRepository;
        private CourseRepository _courseRepository;

        public HomeController(ProjectContext context)
        {
            _lessonRepository = new LessonRepository(context);
            _courseRepository = new CourseRepository(context);
        }

        public IActionResult Index()
        {
            ViewBag.Courses = _courseRepository.GetAll();
            return View();
        }

        public IActionResult Video()
        {
            return View();
        }

        [Route("/Home/Lesson/{id}")]
        public IActionResult Lesson(Guid id)
        {
            ViewBag.Lesson = _lessonRepository.GetLessonById(id);

            // ReSharper disable once Mvc.ViewNotResolved
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}