using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class Product1Repository : Repository<Product1>, IProduct1Repository
    {
        private readonly ContextDataBase _context;

        public Product1Repository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Product1> products, Log log = null)
        {
            foreach (var c in products)
            {
                _context.Product1s.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Product1 FindByDescription(string description, Log log = null)
        {
            var rst = _context.Product1s.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public decimal FindByPrice(int id, Log log = null)
        {
            decimal rst = Convert.ToDecimal(_context.Product1s.Where(_ => _.Id.Equals(id)).Select(_ => _.Price));
            AddLog(log);
            return rst;
        }

        public Product1 FindByProduct(string code, int grupoId, Log log = null)
        {
            var rst = _context.Product1s.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }


        public List<Product1> FindAllInDate1(DateTime dateProd, Log log = null)
        {
            List<Product1> products = new List<Product1>();

            var productPauta = _context.Product1s;

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
    }
}
