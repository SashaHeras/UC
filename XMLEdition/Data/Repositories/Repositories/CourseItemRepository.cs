using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class CourseItemRepository : Repository<CourseItem>, ICourseItemRepository
    {
        private AppContext _context;

        public CourseItemRepository(AppContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public CourseItem GetCourseItemById(int? courseItemId)
        {
            return GetAll().Where(ci => ci.Id == courseItemId).FirstOrDefault();
        }

        public IQueryable<CourseItem> GetCourseItemsByCourseId(int courseId)
        {
            return GetAll().Where(ci => ci.CourseId == courseId);
        }
    }
}
