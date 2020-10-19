using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DevoFornecedorRepository : Repository<Model.DevoFornecedor>, IDevoFornecedorRepository
    {
        private readonly ContextDataBase _context;

        public DevoFornecedorRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }
    }
}
