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
        private TestRepository _testRepository;
        private CourseItemRepository _courseItemRepository;

        private CourseItemService _courseItemService;
        private TestHistoryService _testHistoryService;
        private TaskHistoryService _taskHistoryService;
        private AnswerHistoryService _answerHistoryService;

        public TestService( 
            TestRepository testRepository, CourseItemRepository courseItemRepository,
            CourseItemService courseItemService, TestHistoryService testHistoryService,
            TaskHistoryService taskHistoryService, AnswerHistoryService answerHistoryService)
        {
            _testRepository = testRepository;
            _courseItemRepository = courseItemRepository;
            _courseItemService = courseItemService;
            _testHistoryService = testHistoryService;
            _taskHistoryService = taskHistoryService;
            _answerHistoryService = answerHistoryService;
        }             

        public Test GetTest(int id)
        {
            return _testRepository.GetTestById(id);
        }

        public Test GetTestByCourseItem(int courseItemId)
        {
            return _testRepository.GetTestByCourseItemId(courseItemId);
        }

        public async void SaveHistory(TestHistory history, List<TaskHistory> taskHistories, List<AnswerHistory> answerHistories)
        {
            history = await _testHistoryService.SaveTestHistory(history);

            taskHistories = await _taskHistoryService.SaveTasksHistory(history, taskHistories);

            await _answerHistoryService.SaveAnswersHistory(taskHistories, answerHistories);
        }
         
        public async Task<Test> CreateNewTest(string testName, int courseId, IQueryable<CourseItem> sameCourseItems)
        {
            CourseItem newCourseItem = new CourseItem()
            {
                TypeId = _courseItemService.GetItemTypeByName("Test").Id,
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
    }
}
