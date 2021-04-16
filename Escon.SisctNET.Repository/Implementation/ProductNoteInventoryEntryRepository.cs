using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryEntryRepository : Repository<Model.ProductNoteInventoryEntry>, IProductNoteInventoryEntryRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryEntryRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
