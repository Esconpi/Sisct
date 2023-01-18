using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class Product2Repository : Repository<Product2>, IProduct2Repository
    {
        private readonly ContextDataBase _context;

        public Product2Repository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Product2 FindByDescription(string description, Log log = null)
        {
            var rst = _context.Product2s.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Product2> FindByGroup(long groupid, Log log = null)
        {
            var rst = _context.Product2s.Where(_ => _.GroupId.Equals(groupid) && _.DateEnd == null).ToList();
            AddLog(log);
            return rst;
        }

        public decimal FindByPrice(long id, Log log = null)
        {
            decimal rst = Convert.ToDecimal(_context.Product2s.Where(_ => _.Id.Equals(id)).Select(_ => _.Price));
            AddLog(log);
            return rst;
        }

        public Product2 FindByProduct(string code, long grupoId, Log log = null)
        {
            var rst = _context.Product2s.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Product2> FindAllInDate(DateTime dateProd, Log log = null)
        {
            List<Product2> products = new List<Product2>();

            var productPauta = _context.Product2s;

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

        public List<Product2> FindByAllGroup(Log log = null)
        {
            var rst = _context.Product2s
                .Include(_ => _.Group)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Product2 FindByProduct(long id, Log log = null)
        {
            var rst = _context.Product2s
                .Where(_ => _.Id.Equals(id))
                .Include(g => g.Group)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
