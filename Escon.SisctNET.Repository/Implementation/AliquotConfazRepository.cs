using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AliquotConfazRepository : Repository<Model.AliquotConfaz>, IAliquotConfazRepository
    {
        private readonly ContextDataBase _context;

        public AliquotConfazRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<AliquotConfaz> FindByAllState(Log log = null)
        {
            var rst = _context.AliquotConfazs
                .Include(so => so.StateOrigem)
                .Include(sd => sd.StateDestino)
                .Include(a => a.Annex)
                .ToList();
            AddLog(log);
            return rst;
        }
    }
}
