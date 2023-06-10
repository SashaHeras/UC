using Antlr.Runtime.Tree;
using Framework.Helper.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using XMLEdition.Core.Services;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;
using XMLEdition.Migrations;

namespace XMLEdition.Controllers
{
    public class TestController : Controller
    {
        private ProjectContext _context = new ProjectContext();
        private CourseItemRepository _courseItemRepository;   
        private TaskRepository _taskRepository;
        private TestService _testService;

        public TestController(ProjectContext context)
        {
            _context = context;
            _courseItemRepository = new CourseItemRepository(context);
            _taskRepository = new TaskRepository(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Test/GoToTest/{courseItemId}")]
        public IActionResult GoToTest(int courseItemId)
        {
            int testId = _testService.GetTestByCourseItem(courseItemId).Id;
            return RedirectToAction("PassTest", new { id = testId });
        }

        [Route("/Test/PassTest/{id}")]
        public IActionResult PassTest(int id)
        {
            var test = _testService.GetTest(id);
            var courseItemId = test.CourseItemId;
            var courseId = _testService.GetCourseItem(courseItemId).CourseId;

            ViewBag.CourseId = courseId;
            ViewBag.Test = test;

            return View();
        }

        public JsonResult GetTasks()
        {
            int testId = Convert.ToInt32(Request.Form["test"]);
            var tasks = _testService.GetTasks(testId);

            return Json(tasks);
        }

        public JsonResult GetAnswers()
        {
            int taskId = Convert.ToInt32(Request.Form["task"]);
            var testAnswers = _context.TaskAnswers.Where(t => t.TaskId == taskId);
            return Json(testAnswers);
        }

        [HttpPost]
        public JsonResult GetMark()
        {
            double total = 0;
            double mark = 0;

            int testId = int.Parse(Request.Form["test"]);
            int userId = int.Parse(Request.Form["userid"]);
            List<TaskHistory> taskHistories = new List<TaskHistory>();
            List<AnswerHistory> answerHistories = new List<AnswerHistory>();

            var tasks = _testService.GetTasks(testId);
            var taskAnswers = Request.Form["answers"].ToString().Split(',')
                                .Select(a => new UserAnswersModel {
                                    TaskId = int.Parse(a.Split('_')[0]),
                                    AnswerId = int.Parse(a.Split('_')[1]),
                                    IsChecked = bool.Parse(a.Split('_')[2])
                                }).ToList();

            foreach (var task in tasks)
            {
                double markForTask = 0;
                var taskAnswerIds = taskAnswers.Where(a => a.TaskId == task.Id).ToList();
                var corectAnswersCount = _testService.PopulateAnswerHistories(taskAnswerIds, task, answerHistories);

                int corAnsCnt = _context.TaskAnswers.Count(ta => ta.TaskId == task.Id && ta.IsCorrect);
                if (corAnsCnt == corectAnswersCount) {
                    markForTask = task.Mark;
                }
                else if (corAnsCnt > corectAnswersCount) {
                    double perOne = task.Mark / corAnsCnt;
                    markForTask = task.Mark / (perOne * corectAnswersCount);
                }

                mark += markForTask;
                total += task.Mark;
                taskHistories.Add(new TaskHistory {
                    TaskId = task.Id,
                    UserMark = markForTask
                });
            }

            var testHistory = new TestHistory
            {
                TestId = testId,
                UserId = userId,
                TotalMark = total,
                Mark = mark
            };
            _testService.SaveHistory(testHistory, taskHistories, answerHistories);

            return Json(mark);
        }

        [Route("/Test/CreateTest/{id}")]
        public IActionResult CreateTest(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SaveTest()
        {
            Test t = new Test();
            CourseItem currecntCourceItem = new CourseItem();
            int courseId = Convert.ToInt32(Request.Form["courseId"]);
            var sameCourseItems = _testService.GetCourseItems(courseId);

            int? testId = Request.Form.Keys.Contains("testId")
                ? Convert.ToInt32(Request.Form["testId"])
                : (int?)null;

            string newName = Request.Form["test"].ToString();

            t = testId.HasValue
                ? await _testService.RenameTest(testId.Value, newName)
                : await _testService.CreateNewTest(newName, courseId, sameCourseItems);

            return Json(t.Id);
        }

        [HttpPost]
        public async Task<JsonResult> SaveAnswers()
        {
            var allAnsws = Request.Form["answers"];
            var allChecked = Request.Form["checked"];
            Dictionary<string, bool> answers = AnswersSpliter(allAnsws, allChecked);

            //
            TestTask tt = _testService.CreateNewTask(
                Request.Form["taskName"].ToString(),
                Convert.ToInt32(Request.Form["orderNumber"]),
                Convert.ToInt32(Request.Form["taskMark"]),
                Convert.ToInt32(Request.Form["testId"])
            ).Result;

            foreach (var answer in answers)
            {
                TaskAnswer ta = new TaskAnswer()
                {
                    Name = answer.Key,
                    IsCorrect = answer.Value,
                    TaskId = tt.Id
                };

                _context.TaskAnswers.Add(ta);
                _context.SaveChanges();
            }

            return Json(true);
        }

        [Route("/Test/EditTest/{id}")]
        public IActionResult EditTest(int id)
        {
            ViewBag.Test = _testService.GetTestByCourseItem(id);
            ViewBag.CourseId = _testService.GetCourseItem(id).CourseId;
            return View();
        }

        [HttpGet]
        [Route("/Test/GetTasks/{id}")]
        public JsonResult GetTasks(int id)
        {
            var result = _testService.GetTasks(id);
            return Json(result);
        }

        [HttpGet]
        [Route("/Test/GetTask/{id}")]
        public JsonResult GetTask(int id)
        {
            var res = _testService.GetTask(id);
            return Json(res);
        }

        [Route("/Test/GetAnswersForEditting/{id}")]
        public JsonResult GetAnswersForEditting(int id)
        {
            var tt = _context.TaskAnswers.Where(t => t.TaskId == id);

            return Json(tt);
        }

        [HttpPost]
        public async Task<JsonResult> SaveEdittedAnswers()
        {
            int taskId = Convert.ToInt32(Request.Form["taskId"]);
            TestTask editedTask = _testService.GetTask(taskId);
            editedTask.Name = Request.Form["taskName"].ToString();
            editedTask.Mark = Convert.ToInt32(Request.Form["taskMark"].ToString());

            var allAnsws = Request.Form["answers"];
            var allChecked = Request.Form["checked"];
            Dictionary<string, bool> answers = AnswersSpliter(allAnsws, allChecked);
            string[] ids = Request.Form["ids"].ToString().Split(',');

            int counter = 0;
            foreach (var answer in answers)
            {
                int currentAnswerId = 0;
                if (int.TryParse(ids[counter], out currentAnswerId) == false)
                {
                    TaskAnswer ta = new TaskAnswer()
                    {
                        Name = answer.Key,
                        IsCorrect = answer.Value,
                        TaskId = editedTask.Id
                    };

                    _context.TaskAnswers.Add(ta);
                    _context.SaveChanges();
                }
                else
                {
                    TaskAnswer ta = _context.TaskAnswers.Where(ta => ta.Id == currentAnswerId).FirstOrDefault();
                    ta.Name = answer.Key;
                    ta.IsCorrect = answer.Value;

                    _context.TaskAnswers.Update(ta);
                    _context.SaveChanges();
                }

                counter++;
            }

            await _testService.UpdateTask(editedTask);

            return Json(true);
        }

        [HttpDelete]
        [Route("/Test/DeleteTask/{taskId}")]
        public JsonResult DeleteTask(int taskId)
        {
            int tid = taskId;

            var task = _testService.GetTask(tid);
            var taskAnswers = _context.TaskAnswers.Where(ta => ta.TaskId == tid).ToList();
            int orderNumber = task.OrderNumber;
            int testId = task.TestId;
            var allTasksAfter = _testService.GetTasksAfter(testId, orderNumber);

            try
            {
                _context.TestTasks.Remove(task);
                _context.SaveChanges();

                _testService.ResetOrderNumbers(orderNumber, allTasksAfter);
            }
            catch(Exception ex)
            {
                return Json(ex.Message);
            }

            return Json(true);
        }

        [HttpPost]
        public JsonResult DeleteAnswer()
        {
            var answer = _context.TaskAnswers.Where(ta => ta.Id == Convert.ToInt32(Request.Form["answerId"])).FirstOrDefault();
            var taskId = answer.TaskId;

            if(answer != null)
            {
                _context.TaskAnswers.Remove(answer);
                _context.SaveChanges();

                var answers = _context.TaskAnswers.Where(ta=>ta.TaskId == taskId).ToList();

                return Json(answers);
            }

            return Json(false);
        }

        public Dictionary<string, bool> AnswersSpliter(string answers, string _checked)
        {
            string[] _answers = answers.Split(',');
            string[] checkedAns = _checked.Split(',');

            Dictionary<string, bool> result = new Dictionary<string, bool>();

            for (int i = 0; i < _answers.Length; i++)
            {
                result.Add(_answers[i].ToString().Replace('|',','), checkedAns[i] == "true" ? true : false);
            }

            return result;
        }
    }
}
