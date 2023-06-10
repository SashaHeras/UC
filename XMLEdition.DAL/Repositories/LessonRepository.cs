using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Repositories
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        private ProjectContext _context;

        public LessonRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        /// <summary>
        /// Method return Lesson by it`s CourseItemId
        /// </summary>
        /// <param name="courseItemId"></param>
        /// <returns></returns>
        public Lesson GetLessonByCourseItemId(int courseItemId)
        {
            return GetAll().Where(l => l.CourseItemId == courseItemId).FirstOrDefault();
        }

        public Lesson GetLessonById(Guid id)
        {
            return GetAll().Where(l => l.Id == id).FirstOrDefault();
        }
    }
}
