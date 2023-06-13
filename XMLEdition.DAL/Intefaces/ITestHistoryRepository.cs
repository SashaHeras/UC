using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Intefaces
{
    public interface ITestHistoryRepository : ISingletoneService, IRepository<TestHistory>
    {

    }
}
