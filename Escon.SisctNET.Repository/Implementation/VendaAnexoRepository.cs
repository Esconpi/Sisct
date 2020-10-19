using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class VendaAnexoRepository : Repository<Model.VendaAnexo>, IVendaAnexoRepository
    {
        private readonly ContextDataBase _context;

        public VendaAnexoRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
