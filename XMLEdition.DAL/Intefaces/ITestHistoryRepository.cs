using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLEdition.DAL.Entities;
using XMLEdition.DAL.Infrastructure;
using XMLEdition.DAL.Interfaces;

namespace XMLEdition.DAL.Intefaces
{
    public interface ITestHistoryRepository : ISingletoneService, IRepository<TestHistory>
    {

    }
}
