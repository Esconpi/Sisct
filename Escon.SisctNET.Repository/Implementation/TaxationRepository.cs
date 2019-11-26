using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Escon.SisctNET.Repository.Implementation
{
    public class TaxationRepository : Repository<Model.Taxation>, ITaxationRepository
    {
        private readonly ContextDataBase _context;
        

        public TaxationRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Taxation FindByCode(string code, DateTime data, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Taxation rls = null;
            //var rst = _context.Taxations.Where(_ => _.Code.Equals(code) && _.DateStart <= data && _.DateEnd == null).FirstOrDefault();
            var rst = _context.Taxations.Where(_ => _.Code.Equals(code) && _.DateEnd.Equals(null)).FirstOrDefault();
            var t = _context.Taxations.Where(_ => _.Code.Equals(code));
            
            foreach(var d in t)
            {
                var dataInicial = DateTime.Compare(d.DateStart, data);
                
                if (dataInicial <= 0 && d.DateEnd == null)
                {
                    rls = d;
                    break;
                }else if (dataInicial <= 0 && DateTime.Compare(Convert.ToDateTime(d.DateEnd), data) > 0 )
                {
                    rls = d;
                    break;
                }
                
                
            }

            AddLog(log);
            return rls;
        }

        public Taxation FindByCode2(string code2, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code2.Equals(code2)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Taxation> FindByCompany(int companyId, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.CompanyId.Equals(companyId));
            AddLog(log);
            return rst.ToList();
        }
    }
}
