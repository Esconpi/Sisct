﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteInventoryEntryRepository : Repository<Model.ProductNoteInventoryEntry>, IProductNoteInventoryEntryRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteInventoryEntryRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task CreateRange(List<ProductNoteInventoryEntry> products, Log log = null)
        {
            _context.ProductNoteInventoryEntries.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public List<ProductNoteInventoryEntry> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
              .Where(_ => _.CompanyId.Equals(companyId))
              .Include(c => c.Company)
              .ToList();
            AddLog(log);
            return rst;
        }

        public List<ProductNoteInventoryEntry> FindByNote(string chave, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
               .Where(_ => _.Chave.Equals(chave))
               .Include(c => c.Company)
               .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNoteInventoryEntry> FindByNotes(long companyId, string year, string month, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
              .Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
              .Include(c => c.Company)
              .ToList();

            var chaves = rst.Select(s => s.Chave).Distinct().ToList();

            List<ProductNoteInventoryEntry> notas = new List<ProductNoteInventoryEntry>();

            foreach (var chave in chaves)
            {
                var nn = rst.Where(_ => _.Chave.Equals(chave)).FirstOrDefault();
                notas.Add(nn);
            }

            AddLog(log);
            return notas.ToList();
        }

        public List<ProductNoteInventoryEntry> FindByNotes(long companyId, string year, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
               .Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year))
               .Include(c => c.Company)
               .ToList();

            AddLog(log);

            return rst;
        }

        public List<ProductNoteInventoryEntry> FindByPeriod(long companyId, System.DateTime inicio, System.DateTime fim, Log log = null)
        {
            var rst = _context.ProductNoteInventoryEntries
              .Where(_ => _.CompanyId.Equals(companyId) && _.Dhemi >= inicio && _.Dhemi < fim.AddDays(1))
              .Include(c => c.Company)
              .ToList();

            AddLog(log);

            return rst;
        }
    }
}
