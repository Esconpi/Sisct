using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class InternalAliquotConfazRepository : Repository<Model.InternalAliquotConfaz>, IInternalAliquotConfazRepository
    {
        private readonly ContextDataBase _context;

        public InternalAliquotConfazRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<InternalAliquotConfaz> FindByAllState(Log log = null)
        {
            var rst = _context.InternalAliquotConfazs
               .Include(s => s.State)
               .Include(a => a.Annex)
               .ToList();
            AddLog(log);
            return rst;
        }
    }
}
