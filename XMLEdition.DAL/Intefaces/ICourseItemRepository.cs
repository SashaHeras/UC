using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;

namespace XMLEdition.DAL.Interfaces
{
    public interface ICourseItemRepository : ISingletoneService, IRepository<CourseItem>
    {
        public IQueryable<CourseItem> GetCourseItemsByCourseId(int courseId);

        public CourseItem GetCourseItemById(int? courseItemId);
    }
}
