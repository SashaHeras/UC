using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;

namespace XMLEdition.DAL.Interfaces
{
    public interface ITaskRepository : ISingletoneService, IRepository<TestTask>
    {
        public IQueryable<TestTask> GetTaskByTestId(int testId);

        public TestTask GetTaskById(int taskId);
    }
}
