using XMLEdition.Data.Infrastructure;

namespace XMLEdition.Data.Repositories.Interfaces
{
    public interface ITaskRepository : ISingletoneService, IRepository<TestTask>
    {
        public IQueryable<TestTask> GetTaskByTestId(int testId);

        public TestTask GetTaskById(int taskId);
    }
}
