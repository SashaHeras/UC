﻿using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Repositories
{
    public class TaskRepository : Repository<TestTask>, ITaskRepository
    {
        private ProjectContext _context;

        public TaskRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public TestTask GetTaskById(int taskId)
        {
            return GetAll().Where(t => t.Id == taskId).FirstOrDefault();
        }

        public List<TestTask> GetTasksOfTestBiggerThanOrder(int testId, int order)
        {
            return _context.TestTasks.Where(tt => tt.TestId == testId && tt.OrderNumber > order).ToList();
        }

        public IQueryable<TestTask> GetTaskByTestId(int testId)
        {
            return GetAll().Where(t=>t.TestId == testId);
        }

        public bool DeleteTask(TestTask task)
        {
            try
            {
                _context.TestTasks.Remove(task);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
