using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;

namespace XMLEdition.DAL.Interfaces
{
    public interface ILessonRepository : ISingletoneService, IRepository<Lesson>
    {
        public Lesson GetLessonById(Guid id);

        public Lesson GetLessonByCourseItemId(int id);
    }
}
