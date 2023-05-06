using Antlr.Runtime.Tree;
using Framework.Helper.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using XMLEdition.Data;
using XMLEdition.Data.Repositories.Interfaces;
using XMLEdition.Data.Repositories.Repositories;
using XMLEdition.Migrations;

namespace XMLEdition.Controllers
{
    public class TestController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();
        private CourseItemRepository _courseItemRepository;
        private TaskRepository _taskRepository;

        public TestController(Data.AppContext context)
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
            int testId = _context.Tests.Where(t => t.CourseItemId == courseItemId).FirstOrDefault().Id;

            return RedirectToAction("PassTest", new { id = testId });
        }

        [Route("/Test/PassTest/{id}")]
        public IActionResult PassTest(int id)
        {
            var test = _context.Tests.Where(t => t.Id == id).FirstOrDefault();
            var courseItemId = test.CourseItemId;
            var courseId = _context.CourseItem.Where(ci => ci.Id == courseItemId).FirstOrDefault().CourseId;

            ViewBag.CourseId = courseId;
            ViewBag.Test = test;

            return View();
        }

        public JsonResult GetTasks()
        {
            int testId = Convert.ToInt32(Request.Form["test"]);
            var tt = _taskRepository.GetTaskByTestId(testId).OrderBy(t => t.OrderNumber);

            return Json(tt);
        }

        public JsonResult GetAnswers()
        {
            var tt = _context.TaskAnswers.Where(t => t.TaskId == Convert.ToInt32(Request.Form["task"]));

            return Json(tt);
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

            var tasks = _taskRepository.GetTaskByTestId(testId);
            var taskAnswers = Request.Form["answers"].ToString().Split(',')
                                .Select(a => new {
                                    TaskId = int.Parse(a.Split('_')[0]),
                                    AnswerId = int.Parse(a.Split('_')[1]),
                                    IsChecked = bool.Parse(a.Split('_')[2])
                                }).ToList();

            foreach (var task in tasks)
            {
                double markForTask = 0;
                var taskAnswerIds = taskAnswers.Where(a => a.TaskId == task.Id).ToList();
                int cntRight = 0;

                foreach (var taskAnswer in taskAnswerIds)
                {
                    bool isRight = _context.TaskAnswers.Where(ta => ta.TaskId == task.Id && ta.Id == taskAnswer.AnswerId).First().IsCorrect;
                    if (taskAnswer.IsChecked == isRight) {
                        cntRight++;
                        answerHistories.Add(new AnswerHistory
                        {
                            TaskId = task.Id,
                            AnswerId = taskAnswer.AnswerId,
                            IsCorrect = true
                        });
                    }
                    else {
                        answerHistories.Add(new AnswerHistory
                        {
                            TaskId = task.Id,
                            AnswerId = taskAnswer.AnswerId,
                            IsCorrect = false
                        });
                    }
                }

                int corAnsCnt = _context.TaskAnswers.Count(ta => ta.TaskId == task.Id && ta.IsCorrect);
                if (corAnsCnt == cntRight) {
                    markForTask = task.Mark;
                }
                else if (cntRight == 0) {
                    markForTask = 0;
                }
                else if (corAnsCnt > cntRight) {
                    double perOne = task.Mark / corAnsCnt;
                    markForTask = task.Mark / (perOne * cntRight);
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

            _context.TestHistory.Add(testHistory);
            _context.SaveChanges();

            foreach (var taskHistory in taskHistories)
            {
                taskHistory.TestHistoryId = testHistory.Id;
                _context.TaskHistory.Add(taskHistory);
            }

            foreach (var answerHistory in answerHistories)
            {
                answerHistory.TaskHistoryId = taskHistories.First(t => t.TaskId == answerHistory.TaskId).Id;
                _context.AnswerHistory.Add(answerHistory);
            }

            _context.SaveChanges();

            return Json(mark);
        }

        [Route("/Test/CreateTest/{id}")]
        public IActionResult CreateTest(int id)
        {
            ViewBag.CourseId = id;

            return View();
        }

        [HttpPost]
        public JsonResult SaveTest()
        {
            Test t = new Test();
            CourseItem currecntCourceItem = new CourseItem();
            int courseId = Convert.ToInt32(Request.Form["courseId"]);

            var sameCourseItems = _courseItemRepository.GetCourseItemsByCourseId(courseId).OrderBy(ci => ci.OrderNumber);

            if (Request.Form.Keys.Contains("testId") == false)
            {
                currecntCourceItem = new CourseItem()
                {
                    TypeId = _context.CourseItemTypes.Where(cit => cit.Name == "Test").FirstOrDefault().Id,
                    CourseId = Convert.ToInt32(Request.Form["courseId"]),
                    DateCreation = DateTime.Now,
                    OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1
                };

                _courseItemRepository.AddAsync(currecntCourceItem);

                t = new Test()
                {
                    Name = Request.Form["test"].ToString(),
                    CourseItemId = currecntCourceItem.Id
                };

                _context.Tests.Add(t);
                _context.SaveChanges();
            }
            else
            {
                t = _context.Tests.Where(t => t.Id == Convert.ToInt32(Request.Form["testId"])).FirstOrDefault();

                t.Name = Request.Form["test"].ToString();

                _context.Tests.Update(t);
                _context.SaveChanges();
            }

            return Json(t.Id);
        }

        [HttpPost]
        public JsonResult SaveAnswers()
        {
            var allAnsws = Request.Form["answers"];
            var allChecked = Request.Form["checked"];
            Dictionary<string, bool> answers = AnswersSpliter(allAnsws, allChecked);

            TestTask tt = new TestTask()
            {
                Name = Request.Form["taskName"].ToString(),
                OrderNumber = Convert.ToInt32(Request.Form["orderNumber"]),
                Mark = Convert.ToInt32(Request.Form["taskMark"]),
                TestId = Convert.ToInt32(Request.Form["testId"])
            };

            _taskRepository.AddAsync(tt);

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
            var test = _context.Tests.Where(t=>t.CourseItemId == id).FirstOrDefault();
            ViewBag.Test = test;
            ViewBag.CourseId = _courseItemRepository.GetCourseItemById(id).CourseId;

            return View();
        }

        [HttpGet]
        [Route("/Test/GetTasks/{id}")]
        public JsonResult GetTasks(int id)
        {
            var res = _taskRepository.GetTaskByTestId(id);
            return Json(res);
        }

        [HttpGet]
        [Route("/Test/GetTask/{id}")]
        public JsonResult GetTask(int id)
        {
            var res = _context.TestTasks.Where(tt => tt.Id == id).FirstOrDefault();

            return Json(res);
        }

        [Route("/Test/GetAnswersForEditting/{id}")]
        public JsonResult GetAnswersForEditting(int id)
        {
            var tt = _context.TaskAnswers.Where(t => t.TaskId == id);

            return Json(tt);
        }

        [HttpPost]
        public JsonResult SaveEdittedAnswers()
        {
            int taskId = Convert.ToInt32(Request.Form["taskId"]);
            TestTask editedTask = _context.TestTasks.Where(tt => tt.Id == taskId).FirstOrDefault();
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

            _context.TestTasks.Update(editedTask);
            _context.SaveChanges();

            return Json(true);
        }

        [HttpDelete]
        [Route("/Test/DeleteTask/{taskId}")]
        public JsonResult DeleteTask(int taskId)
        {
            int tid = taskId;

            var task = _taskRepository.GetTaskById(tid);
            var taskAnswers = _context.TaskAnswers.Where(ta=>ta.TaskId == tid).ToList();
            int orderNumber = task.OrderNumber;
            int testId = task.TestId;
            var allTasksAfter = _context.TestTasks.Where(tt => tt.TestId == testId && tt.OrderNumber > orderNumber).ToList();

            try
            {
                _context.TestTasks.Remove(task);
                _context.SaveChanges();

                foreach(var ans in allTasksAfter)
                {
                    ans.OrderNumber = orderNumber;
                    orderNumber++;

                    _context.TestTasks.Update(ans);
                    _context.SaveChanges();
                }
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
