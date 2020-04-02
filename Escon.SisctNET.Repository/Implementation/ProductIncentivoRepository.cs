using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductIncentivoRepository : Repository<Model.ProductIncentivo>, IProductIncentivoRepository
    {
        private readonly ContextDataBase _context;

        public ProductIncentivoRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public ProductIncentivo FindByProduct(int company, string code, Log log)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company) &&  _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByProducts(int id, string year, string month, Log log = null)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
            AddLog(log);
            return result;
        }
    }
}
