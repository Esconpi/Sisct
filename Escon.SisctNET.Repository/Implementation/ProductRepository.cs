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

        public List<Product> FindByAllGroup(Log log = null)
        {
            var rst = _context.Products
               .Include(g => g.Group)
               .ToList();
            AddLog(log);
            return rst;
        }

        public List<Product> FindByGroup(long groupid, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.GroupId.Equals(groupid) && _.DateEnd == null).ToList();
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

        public Product FindByProduct(string code, long grupoId, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
