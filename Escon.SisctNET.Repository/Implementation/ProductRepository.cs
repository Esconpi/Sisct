﻿using System;
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

        public List<Product2> FindAllInDate2(DateTime dateProd, Log log = null)
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

        public Product FindByProduct(string code, int grupoId, string description, Log log = null)
        {
            var rst = _context.Products.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.Description.Equals(description) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
