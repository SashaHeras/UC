using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Intefaces
{
    public interface IAnswerRepository : ISingletoneService, IRepository<TaskAnswer>
    {
        public TaskAnswer GetAnswerById(int id);

        public bool DeleteAnswer(TaskAnswer answer);

        public IQueryable<TaskAnswer> GetAnswersByTaskId(int taskId);

        public int GetCountOfCorrectAnswers(int taskId);
    }
}
