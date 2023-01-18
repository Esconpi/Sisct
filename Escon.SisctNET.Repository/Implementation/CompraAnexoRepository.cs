using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CompraAnexoRepository : Repository<Model.CompraAnexo>, ICompraAnexoRepository
    {
        private readonly ContextDataBase _context;

        public CompraAnexoRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<CompraAnexo> FindByComprasTax(long taxAnexo, Log log = null)
        {
            var rst = _context.CompraAnexos.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
