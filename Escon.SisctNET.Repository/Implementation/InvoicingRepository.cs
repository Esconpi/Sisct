using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class InvoicingRepository : Repository<Invoicing>, IInvoicingRepository
    {
        private readonly ContextDataBase _context;

        public InvoicingRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

    }
}
