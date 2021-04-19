using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryEntryRepository : Repository<Model.ProductNoteInventoryEntry>, IProductNoteInventoryEntryRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryEntryRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<ProductNoteInventoryEntry> products, Log log = null)
        {
            foreach (var p in products)
            {
                _context.ProductNoteInventoryEntries.Add(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<ProductNoteInventoryEntry> FindByNote(int noteId, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
                 .Where(_ => _.NoteInventoryEntryId.Equals(noteId))
                 .Include(c => c.NoteInventoryEntry.Company)
                 .ToList();
            AddLog(log);
            return rst.ToList();
        }
    }
}
