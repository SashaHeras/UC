using XMLEdition.Data.Repositories.Interfaces;

namespace XMLEdition.Data.Repositories.Repositories
{
    public class TaskRepository : Repository<TestTask>, ITaskRepository
    {
        private AppContext _context;

        public TaskRepository(AppContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public TestTask GetTaskById(int taskId)
        {
            return GetAll().Where(t => t.Id == taskId).FirstOrDefault();
        }

        public IQueryable<TestTask> GetTaskByTestId(int testId)
        {
            return GetAll().Where(t=>t.TestId == testId);
        }
    }
}
