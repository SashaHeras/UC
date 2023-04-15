using Microsoft.AspNetCore.Mvc;

namespace XMLEdition.Controllers
{
    public class AuthorController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateCourse()
        {
            return View();
        }
    }
}
