using Framework.Helper.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Implementation;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using XMLEdition.Data;
using XMLEdition.Migrations;

namespace XMLEdition.Controllers
{
    public class TestController : Controller
    {
        private Data.AppContext _context = new Data.AppContext();

        public IActionResult Index()
        {
            return View();
        }

        [Route("/Test/PassTest/{id}")]
        public IActionResult PassTest(int id)
        {
            ViewBag.Test = _context.Tests.Where(t => t.Id == id).FirstOrDefault();

            return View();
        }

        public JsonResult GetTasks()
        {
            var tt = _context.TestTasks.Where(t => t.TestId == Convert.ToInt32(Request.Form["test"])).OrderBy(t => t.OrderNumber);

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
            int testId = Convert.ToInt32(Request.Form["test"]);
            TestHistory testHistory = new TestHistory();
            List<TaskHistory> taskHistories = new List<TaskHistory>();
            List<AnswerHistory> answerHistories = new List<AnswerHistory>();

            testHistory.TestId = testId;
            testHistory.UserId = Convert.ToInt32(Request.Form["userid"]);
            // Add to testHistory user id

            List<TestTask> tasks = _context.TestTasks.Where(tt => tt.TestId == testId).ToList();

            var answers = Request.Form["answers"].ToString().Split(',');
            for (int i = 0; i < tasks.Count; i++)
            {
                TaskHistory taskHistory = new TaskHistory();
                taskHistory.TaskId = tasks[i].Id;
                taskHistory.TestHistoryId = 0;

                total += tasks[i].Mark;
                double markForTask = 0;
                var taskAnswers = answers.Where(a => a.Split('_')[0].Equals(tasks[i].Id.ToString()) == true).ToList();
                int cntRight = 0;

                for (int j = 0; j < taskAnswers.Count(); j++)
                {
                    AnswerHistory answerHistory = new AnswerHistory();
                    answerHistory.TaskId = tasks[i].Id;
                    answerHistory.TaskHistoryId = 0;

                    string isChecked = taskAnswers[j].ToString().Split('_')[2].ToString();
                    answerHistory.AnswerId = Convert.ToInt32(taskAnswers[j].ToString().Split('_')[1].ToString());

                    bool isRight = _context.TaskAnswers.Where(ta => ta.TaskId == tasks[i].Id && ta.Id == answerHistory.AnswerId).First().IsCorrect;
                    if(Convert.ToBoolean(isChecked) == isRight)
                    {
                        answerHistory.IsCorrect = true;
                        cntRight++;
                    }
                    else
                    {
                        answerHistory.IsCorrect = false;
                    }

                    answerHistories.Add(answerHistory);
                }

                int corAnsCnt = _context.TaskAnswers.Where(ta => ta.TaskId == tasks[i].Id && ta.IsCorrect == true).Count();
                if (corAnsCnt == cntRight)
                {
                    markForTask += tasks[i].Mark;
                }
                else if(cntRight == 0)
                {
                    markForTask += 0;
                }
                else if (corAnsCnt > cntRight)
                {
                    double perOne = tasks[i].Mark / (double)corAnsCnt;
                    markForTask += tasks[i].Mark / (double)(perOne * cntRight);
                }
                else
                {

                }

                mark += markForTask;

                taskHistory.UserMark = markForTask;
                taskHistories.Add(taskHistory);
            }

            testHistory.TotalMark = total;
            testHistory.Mark = mark;
            _context.TestHistory.Add(testHistory);
            _context.SaveChanges();

            for (int i = 0; i < taskHistories.Count; i++) 
            {
                taskHistories[i].TestHistoryId = testHistory.Id;
                _context.TaskHistory.Add(taskHistories[i]);
                _context.SaveChanges();
            }

            for (int i = 0; i < answerHistories.Count; i++)
            {
                answerHistories[i].TaskHistoryId = taskHistories.Where(t => t.TaskId == answerHistories[i].TaskId).FirstOrDefault().Id;
                _context.AnswerHistory.Add(answerHistories[i]);
                _context.SaveChanges();
            }

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

            var sameCourseItems = _context.CourseItem.Where(ci => ci.CourseId == Convert.ToInt32(Request.Form["courseId"])).OrderBy(ci => ci.OrderNumber);

            if (Request.Form.Keys.Contains("testId") == false)
            {
                currecntCourceItem = new CourseItem()
                {
                    TypeId = _context.CourseItemTypes.Where(cit => cit.Name == "Test").FirstOrDefault().Id,
                    CourseId = Convert.ToInt32(Request.Form["courseId"]),
                    DateCreation = DateTime.Now,
                    OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1
                };

                _context.CourseItem.Add(currecntCourceItem);
                _context.SaveChanges();

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
            TestTask tt = new TestTask()
            {
                Name = Request.Form["taskName"].ToString(),
                OrderNumber = Convert.ToInt32(Request.Form["orderNumber"]),
                Mark = Convert.ToInt32(Request.Form["taskMark"]),
                TestId = Convert.ToInt32(Request.Form["testId"])
            };

            _context.TestTasks.Add(tt);
            _context.SaveChanges();

            var allAnsws = Request.Form["answers"];
            var allChecked = Request.Form["checked"];
            Dictionary<string, bool> answers = AnswersSpliter(allAnsws, allChecked);

            foreach(var answer in answers)
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
            ViewBag.CourseId = _context.CourseItem.Where(c => c.Id == id).FirstOrDefault().CourseId;

            return View();
        }

        [HttpGet]
        [Route("/Test/GetTasks/{id}")]
        public JsonResult GetTasks(int id)
        {
            var res = _context.TestTasks.Where(tt => tt.TestId == id).ToList();
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
            TestTask editedTask = _context.TestTasks.Where(tt => tt.Id == Convert.ToInt32(Request.Form["taskId"])).FirstOrDefault();
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
