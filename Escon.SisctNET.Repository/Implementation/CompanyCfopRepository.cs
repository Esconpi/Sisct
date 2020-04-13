using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CompanyCfopRepository : Repository<Model.CompanyCfop>, ICompanyCfopRepository
    {
        private readonly ContextDataBase _context;

        public CompanyCfopRepository(ContextDataBase context, IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
        }

        public List<CompanyCfop> FindByCfopActive(int companyId, string type, string typeCfop, Log log = null)
        {
            List<CompanyCfop> result = null;

            if (type.Equals("resumocfop") && typeCfop.Equals("all"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).ToList();
            }
            else if ((type.Equals("venda") || type.Equals("anexo") || type.Equals("foraAnexo")) && typeCfop.Equals("venda"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(1)).ToList();
            }
            else if (type.Equals("incentivo") && typeCfop.Equals("vendaSt"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(5)).ToList();
            }
            else if (type.Equals("venda") && typeCfop.Equals("transferencia"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(2)).ToList();

            }
            else if((type.Equals("venda") || type.Equals("incentivo")) && typeCfop.Equals("devolucao de compra"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(3)).ToList();
            }
            else if (type.Equals("entrada") && typeCfop.Equals("compra"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && (_.CfopTypeId.Equals(6) || _.CfopTypeId.Equals(7))).ToList();
            }
            else if (type.Equals("incentivo") && typeCfop.Equals("devolução de venda"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(7)).ToList();
            }
            else if (type.Equals("incentivo") && typeCfop.Equals("venda"))
            {
                result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true) && _.CfopTypeId.Equals(1)).ToList();
            }

            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCompany(int companyId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId));
            AddLog(log);
            return result.ToList();
        }

        public CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.CfopId.Equals(cfopId)).FirstOrDefault();
            AddLog(log);
            return result;

        }

    }
}