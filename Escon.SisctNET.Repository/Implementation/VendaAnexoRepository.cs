using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class VendaAnexoRepository : Repository<Model.VendaAnexo>, IVendaAnexoRepository
    {
        private readonly ContextDataBase _context;

        public VendaAnexoRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<VendaAnexo> FindByVendasTax(long taxAnexo, Log log = null)
        {
            var rst = _context.VendaAnexos.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
