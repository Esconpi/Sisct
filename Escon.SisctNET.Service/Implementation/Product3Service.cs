using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class Product3Service : IProduct3Service
    {
        private readonly IProduct3Repository _repository;

        public Product3Service(IProduct3Repository repository)
        {
            _repository = repository;
        }

        public void Create(List<Product3> products, Log log = null)
        {
            _repository.Create(products, log);
        }

        public Product3 Create(Product3 entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Product3> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Product3> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Product3 FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description, log);
        }

        public List<Product3> FindByGroup(long groupid, Log log = null)
        {
            return _repository.FindByGroup(groupid, log);
        }

        public Product3 FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public decimal FindByPrice(long id, Log log = null)
        {
            return _repository.FindByPrice(id, log);
        }

        public Product3 FindByProduct(string code, long grupoId, Log log = null)
        {
            return _repository.FindByProduct(code, grupoId, log);
        }

        public Product3 Update(Product3 entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public void Update(List<Product3> products, Log log = null)
        {
            _repository.Update(products, log);
        }

        public List<Product3> FindAllInDate2(DateTime dateProd, Log log = null)
        {
            return _repository.FindAllInDate2(dateProd, log);
        }

        public List<Product3> FindByAllGroup(Log log = null)
        {
            return _repository.FindByAllGroup(log);
        }

        public Product3 FindByProduct(long id, Log log = null)
        {
            return _repository.FindByProduct(id, log);
        }
    }
}
