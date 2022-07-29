using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class InternalAliquotRepository : Repository<Model.InternalAliquot>, IInternalAliquotRepository
    {
        private readonly ContextDataBase _context;

        public InternalAliquotRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<InternalAliquot> FindByAllState(Log log = null)
        {
            var rst = _context.InternalAliquots
                .Include(s => s.State)
                .ToList();
            AddLog(log);
            return rst;
        }
    }
}
