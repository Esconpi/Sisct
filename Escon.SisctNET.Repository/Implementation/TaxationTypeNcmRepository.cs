using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;


namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationTypeNcmRepository : Repository<Model.TaxationTypeNcm>, ITaxationTypeNcmRepository
    {
        private readonly ContextDataBase _context;

        public TaxationTypeNcmRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
