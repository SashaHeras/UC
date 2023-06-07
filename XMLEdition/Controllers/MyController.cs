using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Web.Helpers;
using XMLEdition.Data.Repositories.Interfaces;
using XMLEdition.Data.Repositories.Repositories;

namespace XMLEdition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private CourseItemRepository _courseItemRepository;
        private TaskRepository _taskRepository;

        public MyController(Data.AppContext context)
        {
            _context = context;
            _taskRepository = new TaskRepository(context);
            _courseItemRepository = new CourseItemRepository(context);
        }

        [HttpGet, Route("tasks")]
        public JsonResult GetTasks()
        {
            var data = _taskRepository.GetAll();
            return Json(data);
        }
    }
}
