using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class LogRepository : ILogRepository
    {
        private readonly ContextDataBase _context;
        private readonly IConfiguration _configuration;

        private DbSet<Model.Log> dataSet;

        public LogRepository(ContextDataBase context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            dataSet = _context.Set<Model.Log>();
        }

        public List<Model.Log> FindAll(Model.Log log)
        {
            return dataSet.OrderByDescending(_ => _.Created).ToList();
        }

        public List<Log> FindFunctionality(int functionalityId, Log log)
        {
            return dataSet.Where(_ => _.FunctionalityId.Equals(functionalityId)).OrderByDescending(_ => _.Created).ToList();
        }

        public List<Log> FindUser(int userId, Log log)
        {
            return dataSet.Where(_ => _.PersonId.Equals(userId)).OrderByDescending(_ => _.Created).ToList();
        }
    }
}
