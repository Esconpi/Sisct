using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryExitRepository : Repository<Model.ProductNoteInventoryExit>, IProductNoteInventoryExitRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryExitRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
