using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Intefaces;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;

namespace XMLEdition.Core.Services
{
    public class TestService
    {
        private ProjectContext _context;
        private TestRepository _testRepository;
        private CourseItemRepository _courseItemRepository;
        private TaskRepository _taskRepository;
        private CourseTypeRepository _courseItemTypeRepository;

        public TestService(ProjectContext projectContext, 
            TestRepository testRepository, 
            CourseItemRepository courseItemRepository, 
            TaskRepository taskRepository, 
            CourseTypeRepository courseTypeRepository)
        {
            _context = projectContext;
            _testRepository = testRepository;
            _courseItemRepository = courseItemRepository;
            _taskRepository = taskRepository;
            _courseItemTypeRepository = courseTypeRepository;
        }             

        public Test GetTest(int id)
        {
            return _testRepository.GetTestById(id);
        }

        public TestTask GetTask(int id)
        {
            return _taskRepository.GetTaskById(id);
        }

        public CourseItem GetCourseItem(int id)
        {
            return _courseItemRepository.GetCourseItemById(id);
        }

        public IQueryable<TestTask> GetTasks(int testId)
        {
            return _taskRepository.GetTaskByTestId(testId).OrderBy(t => t.OrderNumber);
        }

        public Test GetTestByCourseItem(int courseItemId)
        {
            return _testRepository.GetTestByCourseItemId(courseItemId);
        }

        public CourseItemType GetItemTypeByName(string name)
        {
            return _courseItemTypeRepository.GetItemTypeByName(name);
        }

        public List<TestTask> GetTasksAfter(int testId, int order)
        {
            return _taskRepository.GetTasksOfTestBiggerThanOrder(testId,order);
        }

        public async void ResetOrderNumbers(int order, List<TestTask> allTasksAfter)
        {
            foreach (var task in allTasksAfter)
            {
                task.OrderNumber = order++;
                //order++;
                await _taskRepository.UpdateAsync(task);
            }
        }

        public async Task<TestTask> UpdateTask(TestTask task)
        {
            return await _taskRepository.UpdateAsync(task);
        }

        public IQueryable<CourseItem> GetCourseItems(int courseId)
        {
            return _courseItemRepository.GetCourseItemsByCourseId(courseId).OrderBy(ci => ci.OrderNumber);
        }

        public int PopulateAnswerHistories(List<UserAnswersModel> taskAnswerIds, TestTask task, List<AnswerHistory> answerHistories)
        {
            var result = 0;

            foreach (var taskAnswer in taskAnswerIds)
            {
                bool isRight = _context.TaskAnswers
                    .Where(ta => ta.TaskId == task.Id && ta.Id == taskAnswer.AnswerId)
                    .First().IsCorrect;

                var answerHistory = new AnswerHistory
                {
                    TaskId = task.Id,
                    AnswerId = taskAnswer.AnswerId,
                    IsCorrect = (taskAnswer.IsChecked == isRight)
                };

                if (answerHistory.IsCorrect)
                {
                    result++; 
                }

                answerHistories.Add(answerHistory);
            }

            return result;
        }

        public async void SaveHistory(TestHistory history, List<TaskHistory> taskHistories, List<AnswerHistory> answerHistories)
        {
            _context.TestHistory.Add(history);
            ///
            await _context.SaveChangesAsync();

            foreach (var taskHistory in taskHistories)
            {
                taskHistory.TestHistoryId = history.Id;
                _context.TaskHistory.Add(taskHistory);
            }

            foreach (var answerHistory in answerHistories)
            {
                answerHistory.TaskHistoryId = taskHistories.First(t => t.TaskId == answerHistory.TaskId).Id;
                _context.AnswerHistory.Add(answerHistory);
            }

            await _context.SaveChangesAsync();
        }
         
        public async Task<Test> CreateNewTest(string testName, int courseId, IQueryable<CourseItem> sameCourseItems)
        {
            CourseItem newCourseItem = new CourseItem()
            {
                TypeId = GetItemTypeByName("Test").Id,
                CourseId = courseId,
                DateCreation = DateTime.Now,
                OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1
            };
            await _courseItemRepository.AddAsync(newCourseItem);

            Test test = new Test()
            {
                Name = testName,
                CourseItemId = newCourseItem.Id
            };
            await _testRepository.AddAsync(test);

            return test;
        }

        public async Task<Test> RenameTest(int testId, string newName)
        {
            Test test = _testRepository.GetTestById(testId);
            test.Name = newName;

            await _testRepository.UpdateAsync(test);

            return test;
        }

        public async Task<TestTask> CreateNewTask(string name, int order, int mark, int testId)
        {
            TestTask testTask = new TestTask()
            {
                Name = name,
                OrderNumber = order,
                Mark = mark,
                TestId = testId
            };

            await _taskRepository.AddAsync(testTask);

            return testTask;
        }
    }
}
