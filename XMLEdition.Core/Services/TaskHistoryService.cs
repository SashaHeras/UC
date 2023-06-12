using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Core.Services
{
    public class TaskHistoryService
    {
        private TaskHistoryRepository _taskHistoryRepository;

        public TaskHistoryService(TaskHistoryRepository taskHistoryRepository) {
            _taskHistoryRepository = taskHistoryRepository;
        }

        public async Task<List<TaskHistory>> SaveTasksHistory(TestHistory history, List<TaskHistory> taskHistories)
        {
            foreach (var taskHistory in taskHistories)
            {
                taskHistory.TestHistoryId = history.Id;
                await _taskHistoryRepository.AddAsync(taskHistory);
            }

            return taskHistories;
        }
    }
}
