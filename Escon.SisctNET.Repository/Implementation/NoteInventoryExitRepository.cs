using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NoteInventoryExitRepository : Repository<Model.NoteInventoryExit>, INoteInventoryExitRepository
    {
        private readonly ContextDataBase _context;

        public NoteInventoryExitRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
