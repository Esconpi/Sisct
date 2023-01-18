using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxProducerRepository : Repository<Model.TaxProducer>, ITaxProducerRepository
    {
        private readonly ContextDataBase _context;

        public TaxProducerRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task CreateRange(List<TaxProducer> taxProducers, Log log = null)
        {
            _context.TaxProducers.AddRange(taxProducers);
            await _context.SaveChangesAsync();
        }

        public List<TaxProducer> FindByTaxs(long companyid, string month, string year, Log log = null)
        {
            var rst = _context.TaxProducers.Where(_ => _.CompanyId.Equals(companyid) && _.MesRef.Equals(month) && _.AnoRef.Equals(year)).ToList();
            AddLog(log);
            return rst;
        }

        public async Task UpdateRange(List<TaxProducer> taxProducers, Log log = null)
        {
            _context.TaxProducers.UpdateRange(taxProducers);
            await _context.SaveChangesAsync();
        }
    }
}
