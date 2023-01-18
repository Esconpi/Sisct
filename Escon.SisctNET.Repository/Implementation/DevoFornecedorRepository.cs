using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DevoFornecedorRepository : Repository<Model.DevoFornecedor>, IDevoFornecedorRepository
    {
        private readonly ContextDataBase _context;

        public DevoFornecedorRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public List<DevoFornecedor> FindByDevoTax(long taxAnexo, Log log = null)
        {
            var rst = _context.DevoFornecedors.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
