using XMLEdition.Data.Infrastructure;

namespace XMLEdition.Data.Repositories.Interfaces
{
    public interface ICourseItemRepository : ISingletoneService, IRepository<CourseItem>
    {
        public IQueryable<CourseItem> GetCourseItemsByCourseId(int courseId);

        public CourseItem GetCourseItemById(int courseItemId);
    }
}
