using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DarRepository : Repository<Dar>, IDarRepository
    {
        private readonly ContextDataBase _context;

        public DarRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task<List<Dar>> FindAllAsync(Log log)
        {
            base.AddLog(log);
            var result = await _context.Dars.ToListAsync();
            return result;
        }
    }
}
