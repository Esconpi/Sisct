using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxAnexoRepository : Repository<Model.TaxAnexo>, ITaxAnexoRepository
    {
        private readonly ContextDataBase _context;

        public TaxAnexoRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public TaxAnexo FindByMonth(int company, string mes, string ano, Log log = null)
        {
            var rst = _context.TaxAnexos.Where(_ => _.CompanyId.Equals(company) && _.MesRef.Equals(mes) && _.AnoRef.Equals(ano)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
