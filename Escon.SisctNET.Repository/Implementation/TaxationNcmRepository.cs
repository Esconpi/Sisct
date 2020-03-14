using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationNcmRepository : Repository<Model.TaxationNcm> , ITaxationNcmRepository
    {
        private readonly ContextDataBase _context;

        public TaxationNcmRepository(ContextDataBase context, IConfiguration configuration)
          : base(context, configuration)
        {
            _context = context;
        }
    }
}
