using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Intefaces;

namespace XMLEdition.DAL.Repositories
{
    public class TestHistoryRepository : Repository<TestHistory>, ITestHistoryRepository
    {
        private ProjectContext _context;

        public TestHistoryRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }
    }
}
