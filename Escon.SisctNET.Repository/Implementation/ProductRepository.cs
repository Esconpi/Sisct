using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
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

        public void Create(List<Product> products, Log log = null)
        {
            foreach (var c in products)
            {
                _context.Products.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Product FindByDescription(string description, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public decimal FindByPrice(int id, Log log = null)
        {
            decimal rst = Convert.ToDecimal(_context.Products.Where(_ => _.Id.Equals(id)).Select(_ => _.Price));
            AddLog(log);
            return rst;
        }
    }
}
