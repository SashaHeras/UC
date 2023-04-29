using XMLEdition.Data.Infrastructure;

namespace XMLEdition.Data.Repositories.Interfaces
{
    public interface ICourseTypeRepository : ISingletoneService, IRepository<CourseItemType>
    {
        public CourseItemType GetTypeById(int id);
    }
}
