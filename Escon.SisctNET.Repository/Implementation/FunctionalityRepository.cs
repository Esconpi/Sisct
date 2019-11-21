using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{

    public class FunctionalityRepository : Repository<Functionality>, IFunctionalityRepository
    {
        private readonly ContextDataBase _context;

        public FunctionalityRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Functionality FindByName(string name, Model.Log log)
        {
            var result = _context.Functionalities.Where(_ => _.Name.Equals(name)).FirstOrDefault();
            AddLog(log);

            return result;
        }
    }
}
