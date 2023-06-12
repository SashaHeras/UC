using Azure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Intefaces;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Repositories
{
    public class AnswerRepository : Repository<TaskAnswer>, IAnswerRepository
    {
        private ProjectContext _context;

        public AnswerRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }

        public TaskAnswer GetAnswerById(int id)
        {
            return _context.TaskAnswers.Where(ta => ta.Id == id).FirstOrDefault();
        }

        public bool DeleteAnswer(TaskAnswer answer)
        {
            try
            {
                _context.TaskAnswers.Remove(answer);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        public IQueryable<TaskAnswer> GetAnswersByTaskId(int taskId)
        {
            return _context.TaskAnswers.Where(ta => ta.TaskId == taskId).AsQueryable<TaskAnswer>();
        }

        public int GetCountOfCorrectAnswers(int taskId)
        {
            return _context.TaskAnswers.Count(ta => ta.TaskId == taskId && ta.IsCorrect);
        }

        public TaskAnswer GetAnswerByIdAndTask(int id, int taskId) {
            return _context.TaskAnswers
                    .Where(ta => ta.TaskId == taskId && ta.Id == id)
                    .First();
        }
    }
}
