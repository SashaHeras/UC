﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using XMLEdition.Data;
using System.IO;
using XMLEdition.Data.Repositories.Repositories;
using Microsoft.WindowsAzure.Storage;
using XMLEdition.Models;

namespace XMLEdition.Controllers
{
    public class CourseController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private CourseRepository _courseRepository;
        private CourseItemRepository _courseItemRepository;
        private CourseTypeRepository _courseTypeRepository;
        private LessonRepository _lessonRepository;

        public CourseController(Data.AppContext context)
        {
            _context = context;
            _courseRepository = new CourseRepository(context);
            _courseItemRepository = new CourseItemRepository(context);
            _courseTypeRepository = new CourseTypeRepository(context);
            _lessonRepository = new LessonRepository(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Course/CreateCourse/{id}")]
        public IActionResult CreateCourse(int id)
        {
            ViewBag.Subjects = _context.CourseSubjects.ToList();

            if (id == 0)
            {
                ViewBag.Course = new Course()
                {
                    Id = 0,
                    Name = "",
                    Price = 0
                };

                return View();
            }
            
            ViewBag.Course = _courseRepository.GetCourse(id);

            return View();
        }

        [HttpGet]
        public JsonResult GetElements()
        {
            int courseId = Convert.ToInt32(Request.Form["id"]);
            var elements = _courseItemRepository.GetCourseItemsByCourseId(courseId);

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
            var result = _courseRepository.GetCourseElementsList(Request.Form["course"].ToString());
            return Json(result);
        }

        [HttpPost]
        public JsonResult SaveCource()
        {
            var form = Request.Form;
            IFormFile file = null;
            if (form.Files.Count != 0)
            {
                file = form.Files[0];
            }

            int courseId = Convert.ToInt32(form["courseId"]);
            Course newCourse = new Course();

            newCourse.Name = form["name"].ToString();
            newCourse.AuthorId = Guid.Parse(form["authorId"].ToString());
            newCourse.Checked = false;
            newCourse.Price = Convert.ToDecimal(form["price"].ToString());
            newCourse.CourseSubjectId = Convert.ToInt32(form["subject"]);
            newCourse.LastEdittingDate = DateTime.Now;

            if (file != null)
            {
                newCourse.PicturePath = SavePictureAsync(file).Result;
            }
            else if(courseId != 0 && file == null) 
            {
                string path = _courseRepository.GetCourse(courseId).PicturePath;
                newCourse.PicturePath = path;
            }

            if (courseId == 0)
            {
                _courseRepository.AddAsync(newCourse);
            }
            else
            {
                newCourse.Id = _courseRepository.GetCourse(courseId).Id;
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

        public async Task<string> SavePictureAsync(IFormFile file)
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
    }
}
