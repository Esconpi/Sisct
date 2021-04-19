using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NoteInventoryEntryRepository : Repository<Model.NoteInventoryEntry>, INoteInventoryEntryRepository
    {
        private readonly ContextDataBase _context;
        private readonly IProductNoteInventoryEntryRepository _productNoteInventoryEntryRepository;

        public NoteInventoryEntryRepository(
            ContextDataBase context, 
            IConfiguration configuration,
            IProductNoteInventoryEntryRepository productNoteInventoryEntryRepository) 
            : base(context, configuration)
        {
            _context = context;
            _productNoteInventoryEntryRepository = productNoteInventoryEntryRepository;
        }

        public List<NoteInventoryEntry> FindByCompany(int companyId, Log log = null)
        {
            var rst = _context.NoteInventoryEntries
               .Where(_ => _.CompanyId.Equals(companyId))
               .Include(c => c.Company)
               .ToList();
            AddLog(log);
            return rst;
        }

        public NoteInventoryEntry FindByNote(string chave, Log log = null)
        {
            var rst = _context.NoteInventoryEntries
                .Where(_ => _.Chave.Equals(chave))
                .Include(c => c.Company)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public NoteInventoryEntry FindByNote(int id, Log log = null)
        {
            var rst = _context.NoteInventoryEntries
              .Where(_ => _.Id.Equals(id))
              .Include(c => c.Company)
              .FirstOrDefault();
            AddLog(log);
            rst.productNoteInventoryEntries = _productNoteInventoryEntryRepository.FindByNote(id);
            return rst;
        }

        public List<NoteInventoryEntry> FindByNotes(int id, string year, string month, Log log = null)
        {
            var rst = _context.NoteInventoryEntries
              .Where(_ => _.CompanyId.Equals(id) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
              .Include(c => c.Company)
              .ToList();
            AddLog(log);
            return rst.ToList();
        }
    }
}
