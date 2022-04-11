using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProduct1Service : IServiceBase<Model.Product1>
    {
        void Create(List<Model.Product1> products, Model.Log log = null);

        Model.Product1 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(long id, Model.Log log = null);

        Model.Product1 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product1> FindAllInDate1(DateTime dateProd, Model.Log log = null);

        Model.Product1 FindByProduct(long id, Model.Log log = null);
    }
}
