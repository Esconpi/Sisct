using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void Create(List<ProductIncentivo> productIncentivos, Log log = null)
        {
            foreach (var p in productIncentivos)
            {
                _context.ProductIncentivos.Add(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<ProductIncentivo> FindByAllProducts(int company, Log log = null)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company)).ToList();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByAllProducts(string company, Log log = null)
        {
            var result = _context.ProductIncentivos.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8))).ToList();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByDate(int company, DateTime date, Log log = null)
        {
            List<ProductIncentivo> products = new List<ProductIncentivo>();

            var productsIncentivo = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company));

            foreach (var prod in productsIncentivo)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(prod.DateStart), date);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), date);

                if (dataInicial <= 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    products.Add(prod);
                    continue;
                }
            }
            return products;
        }

        public List<ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, int company, DateTime date, Log log = null)
        {
            List<ProductIncentivo> products = new List<ProductIncentivo>();

            var productsIncentivo = productIncentivos.Where(_ => _.CompanyId.Equals(company));

            foreach (var prod in productsIncentivo)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(prod.DateStart), date);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), date);

                if (dataInicial <= 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    products.Add(prod);
                    continue;
                }
            }
            return products;
        }

        public ProductIncentivo FindByProduct(int company, string code, string ncm, string cest, Log log)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company) &&  _.Code.Equals(code) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByProducts(int id, string year, string month, Log log = null)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
            AddLog(log);
            return result;
        }

        public void Update(List<ProductIncentivo> productIncentivos, Log log = null)
        {
            foreach (var p in productIncentivos)
            {
                _context.ProductIncentivos.Update(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
