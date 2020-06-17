using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DarDocumentRepository : Repository<DarDocument>, IDarDocumentRepository
    {
        private readonly ContextDataBase _context;

        public DarDocumentRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
