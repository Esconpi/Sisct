using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryExitRepository : Repository<Model.ProductNoteInventoryExit>, IProductNoteInventoryExitRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryExitRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task CreateRange(List<ProductNoteInventoryExit> products, Log log = null)
        {
            _context.ProductNoteInventoryExits.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public List<ProductNoteInventoryExit> FindByCompany(long companyId, Log log = null)
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

        public List<ProductNoteInventoryExit> FindByNotes(long companyId, string year, string month, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
               .Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
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

        public List<ProductNoteInventoryExit> FindByNotes(long companyId, string year, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
                .Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year))
                .Include(c => c.Company)
                .ToList();

            AddLog(log);

            return rst;
        }

        public List<ProductNoteInventoryExit> FindByPeriod(long companyId, System.DateTime inicio, System.DateTime fim, Log log = null)
        {
            var rst = _context.ProductNoteInventoryExits
             .Where(_ => _.CompanyId.Equals(companyId) &&  _.Dhemi >= inicio && _.Dhemi < fim.AddDays(1))
             .Include(c => c.Company)
             .ToList();

            AddLog(log);

            return rst;
        }
    }
}
