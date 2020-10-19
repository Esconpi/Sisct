using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CompraAnexoRepository : Repository<Model.CompraAnexo>, ICompraAnexoRepository
    {
        private readonly ContextDataBase _context;

        public CompraAnexoRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
