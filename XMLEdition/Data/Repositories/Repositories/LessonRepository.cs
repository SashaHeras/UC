using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        private AppContext _context;

        public LessonRepository(AppContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public Lesson GetLessonByCourseItemId(int id)
        {
            return GetAll().Where(l => l.CourseItemId == id).FirstOrDefault();
        }

        public Lesson GetLessonById(Guid id)
        {
            return GetAll().Where(l => l.Id == id).FirstOrDefault();
        }
    }
}
