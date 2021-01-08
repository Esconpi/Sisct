using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxRepository : Repository<Model.Tax>, ITaxRepository
    {
        private readonly ContextDataBase _context;

        public TaxRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Tax FindByMonth(int company, string mes, string ano, Log log = null)
        {
            var rst = _context.Taxes.Where(_ => _.CompanyId.Equals(company) && _.MesRef.Equals(mes) && _.AnoRef.Equals(ano)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Tax FindByMonth(int company, string mes, string ano, string type, Log log = null)
        {
            Tax rst = null;
            if (type.Equals("Icms"))
            {
                rst = _context.Taxes.Where(_ => _.CompanyId.Equals(company) && _.MesRef.Equals(mes) && _.AnoRef.Equals(ano) && _.Icms.Equals(true)).FirstOrDefault();
            }
            else if (type.Equals("PisCofins"))
            {
                rst = _context.Taxes.Where(_ => _.CompanyId.Equals(company) && _.MesRef.Equals(mes) && _.AnoRef.Equals(ano) && _.PisCofins.Equals(true)).FirstOrDefault();
            }
            AddLog(log);
            return rst;
        }
    }
}
