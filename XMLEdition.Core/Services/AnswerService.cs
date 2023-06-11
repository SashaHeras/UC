using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Core.Services
{
    public class AnswerService
    {
        private AnswerRepository _answerRepository;

        public AnswerService() { 
        
        }

        public void RemoveAnswers(IQueryable<TaskAnswer> answers)
        {
            foreach (var answer in answers)
            {
                _answerRepository.DeleteAnswer(answer);
            }
        }

        public IQueryable<TaskAnswer> GetAnswers(int taskId)
        {
            return _answerRepository.GetAnswersByTaskId(taskId);
        }

        public async void AddAnswer(TaskAnswer answer)
        {
            try
            {
                await _answerRepository.AddAsync(answer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetCountOfCorrectAnswers(int taskId)
        {
            return _answerRepository.GetCountOfCorrectAnswers(taskId);
        }
    }
}
