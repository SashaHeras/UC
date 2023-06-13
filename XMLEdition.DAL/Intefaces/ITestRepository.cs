using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Intefaces
{
    public interface ITestRepository : ISingletoneService, IRepository<Test>
    {
        public Test GetTestByCourseItemId(int courseItemId);

        public Test GetTestById(int testId);
    }
}
