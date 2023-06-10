using Microsoft.EntityFrameworkCore;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;

namespace XMLEdition.DAL.Interfaces
{
    public interface ICourseTypeRepository : ISingletoneService, IRepository<CourseItemType>
    {
        public CourseItemType GetTypeById(int id);

        public CourseItemType GetItemTypeByName(string name);
    }
}
