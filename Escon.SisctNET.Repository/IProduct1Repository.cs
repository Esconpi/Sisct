using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct1Repository : IRepository<Model.Product1>
    {
        void Create(List<Model.Product1> products, Model.Log log = null);

        Model.Product1 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(int id, Model.Log log = null);

        Model.Product1 FindByProduct(string code, int grupoId, Model.Log log = null);

        List<Model.Product1> FindAllInDate1(DateTime dateProd, Model.Log log = null);
    }
}
