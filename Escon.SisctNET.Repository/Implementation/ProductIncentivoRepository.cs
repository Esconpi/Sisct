using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task CreateRange(List<ProductIncentivo> products, Log log = null)
        {
            _context.ProductIncentivos.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public List<ProductIncentivo> FindByAllProducts(long company, Log log = null)
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

        public List<ProductIncentivo> FindByDate(long company, DateTime date, Log log = null)
        {
            List<ProductIncentivo> products = new List<ProductIncentivo>();

            var productsIncentivo = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company));

            foreach (var prod in productsIncentivo)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(prod.DateStart), date);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), date);

                if (dataInicial < 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }
                else if (dataInicial < 0 && dataFinal >= 0)
                {
                    products.Add(prod);
                    continue;
                }
            }
            return products;
        }

        public List<ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, DateTime date, Log log = null)
        {
            List<ProductIncentivo> products = new List<ProductIncentivo>();

            var productsIncentivo = productIncentivos;

            foreach (var prod in productsIncentivo)
            {
                var dataInicial = DateTime.Compare(Convert.ToDateTime(prod.DateStart), date);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), date);

                if (dataInicial < 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }
                else if (dataInicial < 0 && dataFinal >= 0)
                {
                    products.Add(prod);
                    continue;
                }
            }
            return products;
        }

        public ProductIncentivo FindByProduct(long company, string code, string ncm, string cest, Log log)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(company) &&  _.Code.Equals(code) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest)).FirstOrDefault();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByProducts(long id, string year, string month, Log log = null)
        {
            var result = _context.ProductIncentivos.Where(_ => _.CompanyId.Equals(id) && _.Year.Equals(year) && _.Month.Equals(month)).ToList();
            AddLog(log);
            return result;
        }

        public List<ProductIncentivo> FindByProducts(List<ProductIncentivo> productIncentivos, string ncmRaiz, Log log = null)
        {
            List<ProductIncentivo> products = new List<ProductIncentivo>();
            ncmRaiz = ncmRaiz.Replace(".", "");

            int contaChar = ncmRaiz.Length;
            foreach (var n in productIncentivos)
            {
                string substring = "";
                if (contaChar < 8 && n.Ncm.Length > contaChar)
                {
                    substring = n.Ncm.Substring(0, contaChar);
                }
                else
                {
                    substring = n.Ncm;
                }

                if (ncmRaiz.Equals(substring))
                {
                    products.Add(n);
                }
            }

            return products;
        }

        public async Task UpdateRange(List<ProductIncentivo> products, Log log = null)
        {
            _context.ProductIncentivos.UpdateRange(products);
            await _context.SaveChangesAsync();
        }
    }
}
