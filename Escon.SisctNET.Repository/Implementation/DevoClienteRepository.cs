using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DevoClienteRepository : Repository<Model.DevoCliente>, IDevoClienteRepository
    {
        private readonly ContextDataBase _context;

        public DevoClienteRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }
    }
}
