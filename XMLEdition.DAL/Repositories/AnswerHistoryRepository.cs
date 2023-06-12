using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Intefaces;

namespace XMLEdition.DAL.Repositories
{
    public class AnswerHistoryRepository : Repository<AnswerHistory>, IAnswerHistoryRepository
    {
        private ProjectContext _context;

        public AnswerHistoryRepository(ProjectContext repositoryContext) : base(repositoryContext)
        {
            _context = repositoryContext;
        }
    }
}
