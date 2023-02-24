using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
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

        public Product1 FindByProduct(string code, long grupoId, Log log = null)
        {
            var rst = _context.Product1s.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }


        public List<Product1> FindAllInDate(DateTime dateProd, Log log = null)
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

        public Product1 FindByProduct(long id, Log log = null)
        {
            var rst = _context.Product1s
                .Where(_ => _.Id.Equals(id))
                .Include(g => g.Group)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Product1> FindByGroup(long groupid, Log log = null)
        {
            var rst = _context.Product1s.Where(_ => _.GroupId.Equals(groupid) && _.DateEnd == null).ToList();
            AddLog(log);
            return rst;
        }

        public List<Product1> FindByAllGroup(Log log = null)
        {
            var rst = _context.Product1s
              .Include(_ => _.Group)
              .ToList();
            AddLog(log);
            return rst;
        }
    }
}
