using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxProducerRepository : Repository<Model.TaxProducer>, ITaxProducerRepository
    {
        private readonly ContextDataBase _context;

        public TaxProducerRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<TaxProducer> taxProducers, Log log = null)
        {
            foreach (var t in taxProducers)
            {
                _context.TaxProducers.Add(t);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<TaxProducer> FindByTaxs(long companyid, string month, string year, Log log = null)
        {
            var rst = _context.TaxProducers.Where(_ => _.CompanyId.Equals(companyid) && _.MesRef.Equals(month) && _.AnoRef.Equals(year)).ToList();
            AddLog(log);
            return rst;
        }

        public void Update(List<TaxProducer> taxProducers, Log log = null)
        {
            foreach (var t in taxProducers)
            {
                _context.TaxProducers.Update(t);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
