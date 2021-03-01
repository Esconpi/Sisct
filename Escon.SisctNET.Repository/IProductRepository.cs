using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductRepository : IRepository<Model.Product>
    {
        void Create(List<Model.Product> products, Model.Log log = null);

        Model.Product FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(int id, Model.Log log = null);

        Model.Product FindByProduct(string code, int grupoId, string description, Model.Log log = null);

        List<Model.Product> FindAllInDate(DateTime dateProd, Model.Log log = null);
    }
}
