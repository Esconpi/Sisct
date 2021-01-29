using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CountyRepository : Repository<Model.County>, ICountyRepository
    {
        private readonly ContextDataBase _context;

        public CountyRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

    }
}
