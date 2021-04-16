using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NoteInventoryEntryRepository : Repository<Model.NoteInventoryEntry>, INoteInventoryEntryRepository
    {
        private readonly ContextDataBase _context;

        public NoteInventoryEntryRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

    }
}
