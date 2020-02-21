using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CountingTypeRepository : Repository<CountingType>, ICountingTypeRepository
    {
        private readonly ContextDataBase _context;

        public CountingTypeRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
