using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryExitRepository : Repository<Model.ProductNoteInventoryExit>, IProductNoteInventoryExitRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryExitRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<ProductNoteInventoryExit> products, Log log = null)
        {
            foreach (var p in products)
            {
                _context.ProductNoteInventoryExits.Add(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<ProductNoteInventoryExit> FindByNote(int noteId, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
                 .Where(_ => _.NoteInventoryExitId.Equals(noteId))
                 .Include(c => c.NoteInventoryExit.Company)
                 .ToList();
            AddLog(log);
            return rst.ToList();
        }
    }
}
