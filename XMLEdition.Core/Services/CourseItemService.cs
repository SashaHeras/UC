using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;
using XMLEdition.DAL.ViewModels;

namespace XMLEdition.Core.Services
{
    public class CourseItemService
    {
        private CourseTypeRepository _courseItemTypeRepository;
        private CourseItemRepository _courseItemRepository;

        public CourseItemService(CourseItemRepository courseItemRepository, CourseTypeRepository courseTypeRepository) {
            _courseItemTypeRepository = courseTypeRepository;
            _courseItemRepository = courseItemRepository;
        }

        public CourseItem GetCourseItem(int id)
        {
            return _courseItemRepository.GetCourseItemById(id);
        }

        public async Task<CourseItem> CreateNewCourseItem(CreateLessonModel c)
        {
            var courseId = Convert.ToInt32(c.CourseId);
            var sameCourseItems = _courseItemRepository.GetCourseItemsByCourseId(courseId);

            CourseItem newCourceItem = new CourseItem()
            {
                TypeId = _courseItemTypeRepository.GetItemTypeByName("Lesson").Id,
                CourseId = Convert.ToInt32(c.CourseId),
                DateCreation = DateTime.Now,
                OrderNumber = sameCourseItems.Count() > 0 ? sameCourseItems.Last().OrderNumber + 1 : 1,
                StatusId = 2
            };

            await _courseItemRepository.AddAsync(newCourceItem);

            return newCourceItem;
        }

        public IQueryable<CourseItem> GetCourseItems(int courseId)
        {
            return _courseItemRepository.GetCourseItemsByCourseId(courseId).OrderBy(ci => ci.OrderNumber);
        }

        public CourseItemType GetItemTypeByName(string name)
        {
            return _courseItemTypeRepository.GetItemTypeByName(name);
        }

        public CourseItemType GetItemType(int id)
        {
            return _courseItemTypeRepository.GetTypeById(id);
        }

        public async Task<CourseItem> UpdateCourseItem(CourseItem courseItem)
        {
            return await _courseItemRepository.UpdateAsync(courseItem);
        }

        /// <summary>
        /// Return IQuerable of course items
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        public IQueryable<CourseItem> GetElementsByCourseId(int courseId)
        {
            return _courseItemRepository.GetCourseItemsByCourseId(courseId);
        }
    }
}
