using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Intefaces;

namespace XMLEdition.DAL.Repositories
{
    public class TestRepository : Repository<Test>, ITestRepository
    {
        private ProjectContext _context;

        public TestRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public Test GetTestById(int testId)
        {
            return GetAll().Where(t => t.Id == testId).FirstOrDefault();
        }

        public Test GetTestByCourseItemId(int courseItemId)
        {
            return GetAll().Where(t => t.CourseItemId == courseItemId).FirstOrDefault();
        }
    }
}
