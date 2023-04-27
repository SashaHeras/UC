using XMLEdition.Data.Infrastructure;

namespace XMLEdition.Data.Repositories.Interfaces
{
    public interface ILessonRepository : ISingletoneService, IRepository<Lesson>
    {
        public Lesson GetLessonById(Guid id);

        public Lesson GetLessonByCourseItemId(int id);
    }
}
