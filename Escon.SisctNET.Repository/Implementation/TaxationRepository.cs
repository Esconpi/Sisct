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

        public Taxation FindByCode(string code, string cest, DateTime data, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Taxation result = null;
            var taxations = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest));
            
            foreach(var t in taxations)
            {
                var dataInicial = DateTime.Compare(t.DateStart, data);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(t.DateEnd), data);

                if (dataInicial <= 0 && t.DateEnd == null)
                {
                    result = t;
                    break;
                }else if (dataInicial <= 0 && dataFinal > 0 )
                {
                    result = t;
                    break;
                }
                
            }
            AddLog(log);
            return result;
        }

        public Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Taxation result = null;
            var taxationsAll = taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest));

            foreach (var t in taxationsAll)
            {
                var dataInicial = DateTime.Compare(t.DateStart, data);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(t.DateEnd), data);

                if (dataInicial <= 0 && t.DateEnd == null)
                {
                    result = t;
                    break;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    result = t;
                    break;
                }

            }
            AddLog(log);
            return result;
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

        public Taxation FindByNcm(string code, string cest, Log log = null)
        {
            var rst = _context.Taxations.Where(_ => _.Code.Equals(code) && _.Cest.Equals(cest) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
