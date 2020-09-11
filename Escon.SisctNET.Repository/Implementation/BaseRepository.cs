using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class BaseRepository : Repository<Model.Base>, IBaseRepository
    {
        private readonly ContextDataBase _context;

        public BaseRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Base FindByName(string name, Log log = null)
        {
            AddLog(log);
            return _context.Bases.Where(_ => _.Name.Equals(name)).FirstOrDefault();
        }
    }
}
