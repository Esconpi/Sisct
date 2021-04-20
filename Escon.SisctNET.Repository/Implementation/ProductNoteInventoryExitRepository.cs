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

        public List<ProductNoteInventoryExit> FindByCompany(int companyId, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
              .Where(_ => _.CompanyId.Equals(companyId))
              .Include(c => c.Company)
              .ToList();
            AddLog(log);
            return rst;
        }

        public List<ProductNoteInventoryExit> FindByNote(string chave, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
               .Where(_ => _.Chave.Equals(chave))
               .Include(c => c.Company)
               .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNoteInventoryExit> FindByNotes(int id, string year, string month, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
               .Where(_ => _.CompanyId.Equals(id) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
               .Include(c => c.Company)
               .ToList();

            var chaves = rst.Select(s => s.Chave).Distinct().ToList();

            List<ProductNoteInventoryExit> notas = new List<ProductNoteInventoryExit>();
 
            foreach(var chave in chaves) {
                var nn = rst.Where(_ => _.Chave.Equals(chave)).FirstOrDefault();
                notas.Add(nn);
            }

            AddLog(log);
            return notas.ToList();
        }
    }
}
