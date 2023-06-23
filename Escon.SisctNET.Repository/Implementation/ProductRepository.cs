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

        public List<Product> FindAllInDate(DateTime data, Log log = null)
        {
            var rst = _context.Products.Where(_ => (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                                   (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) >= 0))
                                       .ToList();
            AddLog(log);
            return rst;
        }

        public List<Product> FindAllByGroup(Log log = null)
        {
            var rst = _context.Products.Where(_ => _.DateEnd == null)
                                       .Include(g => g.Group)
                                       .ToList();
            AddLog(log);
            return rst;
        }

        public List<Product> FindByGroup(long grupoId, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.GroupId.Equals(grupoId) && _.DateEnd == null).ToList();
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

        public List<Product> FindAllByGroup(long grupoId, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.GroupId.Equals(grupoId))
                                    .Include(g => g.Group)
                                    .ToList();
            AddLog(log);
            return rst;
        }

        public Product FindByProduct(List<Product> products, string code, long grupoId, DateTime data, Log log = null)
        {
            var codes = products.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId)).ToList();
            var rst = codes.Where(_ => (DateTime.Compare(_.DateStart, data) <= 0 && _.DateEnd == null) ||
                                       (DateTime.Compare(_.DateStart, data) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), data) >= 0))
                           .FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
