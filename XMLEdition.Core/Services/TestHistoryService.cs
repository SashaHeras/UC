﻿using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Repositories;

namespace XMLEdition.Core.Services
{
    public class TestHistoryService
    {
        private TestHistoryRepository _testHistoryRepository;

        public TestHistoryService(TestHistoryRepository testHistoryRepository) {
            _testHistoryRepository = testHistoryRepository;
        }

        public async Task<TestHistory> SaveTestHistory(TestHistory history)
        {
            return await _testHistoryRepository.AddAsync(history);
        }
    }
}
