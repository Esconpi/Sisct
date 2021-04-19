using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NoteInventoryExitRepository : Repository<Model.NoteInventoryExit>, INoteInventoryExitRepository
    {
        private readonly ContextDataBase _context;
        private readonly IProductNoteInventoryExitRepository _productNoteInventoryExitRepository;

        public NoteInventoryExitRepository(
            ContextDataBase context,
            IProductNoteInventoryExitRepository productNoteInventoryExitRepository,
            IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
            _productNoteInventoryExitRepository = productNoteInventoryExitRepository;
        }

        public List<NoteInventoryExit> FindByCompany(int companyId, Log log = null)
        {
            var rst = _context.NoteInventoryExits
              .Where(_ => _.CompanyId.Equals(companyId))
              .Include(c => c.Company)
              .ToList();
            AddLog(log);
            return rst;
        }

        public NoteInventoryExit FindByNote(string chave, Log log = null)
        {
            var rst = _context.NoteInventoryExits
               .Where(_ => _.Chave.Equals(chave))
               .Include(c => c.Company)
               .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public NoteInventoryExit FindByNote(int id, Log log = null)
        {
            var rst = _context.NoteInventoryExits
              .Where(_ => _.Id.Equals(id))
              .Include(c => c.Company)
              .FirstOrDefault();
            AddLog(log);
            rst.productNoteInventoryExits = _productNoteInventoryExitRepository.FindByNote(id);
            return rst;
        }

        public List<NoteInventoryExit> FindByNotes(int id, string year, string month, Log log = null)
        {
            var rst = _context.NoteInventoryExits
            .Where(_ => _.CompanyId.Equals(id) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
            .Include(c => c.Company)
            .ToList();
            AddLog(log);
            return rst.ToList();
        }
    }
}
