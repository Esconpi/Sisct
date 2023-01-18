using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ContextDataBase _context;

        public ProductRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Product> FindAllInDate(DateTime dateProd, Log log = null)
        {
            List<Product> products = new List<Product>();

            var productPauta = _context.Products;

            foreach (var prod in productPauta)
            {
                var dataInicial = DateTime.Compare(prod.DateStart, dateProd);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), dateProd);

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

        public Product FindByDescription(string description, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public decimal FindByPrice(long id, Log log = null)
        {
            decimal rst = Convert.ToDecimal(_context.Products.Where(_ => _.Id.Equals(id)).Select(_ => _.Price));
            AddLog(log);
            return rst;
        }

        public Product FindByProduct(string code, long grupoId, string description, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.Description.Equals(description) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Product FindByProduct(long id, Log log = null)
        {
            var rst = _context.Products
                .Where(_ => _.Id.Equals(id))
                .Include(g => g.Group)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
