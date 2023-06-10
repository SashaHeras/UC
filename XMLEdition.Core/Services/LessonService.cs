using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using System.Text.Json;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;

namespace XMLEdition.Core.Services
{
    public class LessonService
    {
        private ProjectContext _context;
        private LessonRepository _lessonRepository;
        private CourseRepository _courseRepository;
        private CourseItemRepository _courseItemRepository;

        public LessonService(ProjectContext context)
        {
            _context = context;
            _courseItemRepository = new CourseItemRepository(context);
            _lessonRepository = new LessonRepository(context);
            _courseRepository = new CourseRepository(context);
        }

        public CourseItem CreateNewCourseItem(CreateLessonModel c)
        {
            var sameCourseItems = _courseItemRepository.GetCourseItemsByCourseId(Convert.ToInt32(c.CourseId)).OrderBy(ci => ci.OrderNumber);

            CourseItem newCourceItem = new CourseItem()
            {
                TypeId = _context.CourseItemTypes.Where(cit => cit.Name == "Lesson").FirstOrDefault().Id,
                CourseId = Convert.ToInt32(c.CourseId),
                DateCreation = DateTime.Now,
                OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1,
                StatusId = 2
            };
            _context.CourseItem.Add(newCourceItem);
            _context.SaveChanges();

            return newCourceItem;
        }

        public void CreateLesson(CourseItem ci, CreateLessonModel clm, string name)
        {
            Lesson newLesson = new Lesson()
            {
                Id = Guid.NewGuid(),
                Theme = clm.Theme,
                Description = clm.Description,
                Body = clm.Body,
                VideoPath = name,
                CourseItemId = ci.Id,
                DateCreation = DateTime.Now.ToShortDateString()
            };

            _lessonRepository.AddAsync(newLesson);
        }

        public Lesson GetLesson(Guid id)
        {
            return _lessonRepository.GetLessonById(id);
        }

        public Course GetCourse(int id)
        {
            return _courseRepository.GetCourse(id);
        }

        public CourseItem GetCourseItem(int id)
        {
            return _courseItemRepository.GetCourseItemById(id);
        }

        public void UpdateLesson(Lesson newLesson)
        {
            Lesson currentLesson = _lessonRepository.GetLessonById(newLesson.Id);

            currentLesson.Theme = newLesson.Theme;
            currentLesson.Description = newLesson.Description;
            currentLesson.Body = newLesson.Body;
            currentLesson.CourseItemId = newLesson.CourseItemId;
            currentLesson.DateCreation = DateTime.Now.ToShortDateString();

            _lessonRepository.UpdateAsync(currentLesson);
        }

        public async Task<CourseItem> UpdateCourseItem(CourseItem courseItem)
        {
            return await _courseItemRepository.UpdateAsync(courseItem);
        }

        public Lesson GetLessonByCourseItem(int courseItemId)
        {
            return _lessonRepository.GetLessonByCourseItemId(courseItemId);
        }
    }
}
