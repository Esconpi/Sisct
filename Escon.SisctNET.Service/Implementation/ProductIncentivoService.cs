using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service.Implementation
{
    public class ProductIncentivoService : IProductIncentivoService
    {

        private readonly IProductIncentivoRepository _repository;

        public ProductIncentivoService(IProductIncentivoRepository repository)
        {
            _repository = repository;
        }

        public ProductIncentivo Create(ProductIncentivo entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<ProductIncentivo> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<ProductIncentivo> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public ProductIncentivo FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public ProductIncentivo FindByProduct(int company, string code, Log log = null)
        {
            return _repository.FindByProduct(company, code, log);
        }

        public List<ProductIncentivo> FindByProducts(int id, string year, string month, Log log = null)
        {
            return _repository.FindByProducts(id, year, month, log);
        }

        public ProductIncentivo Update(ProductIncentivo entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
