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

        public void Create(List<CompraAnexo> compraAnexos, Log log = null)
        {
            foreach (var c in compraAnexos)
            {
                _context.CompraAnexos.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<CompraAnexo> FindByComprasTax(long taxAnexo, Log log = null)
        {
            var rst = _context.CompraAnexos.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }

        public void Update(List<CompraAnexo> compraAnexos, Log log = null)
        {

            foreach (var c in compraAnexos)
            {
                _context.CompraAnexos.Update(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
