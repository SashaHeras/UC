using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class CourseTypeRepository : Repository<CourseItemType>, ICourseTypeRepository
    {
        private AppContext _context;

        public CourseTypeRepository(AppContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public CourseItemType GetTypeById(int id)
        {
            return GetAll().Where(ct => ct.Id == id).FirstOrDefault();
        }
    }
}
