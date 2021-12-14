using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxSupplementRepository : Repository<Model.TaxSupplement>, ITaxSupplementRepository
    {
        private readonly ContextDataBase _context;

        public TaxSupplementRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<TaxSupplement> FindByTaxSupplement(long taxAnexo, Log log = null)
        {
            var rst = _context.TaxSupplements.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
