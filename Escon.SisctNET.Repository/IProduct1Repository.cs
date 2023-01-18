using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct1Repository : IRepository<Model.Product1>
    {
        Model.Product1 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(long id, Model.Log log = null);

        Model.Product1 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product1> FindAllInDate1(DateTime dateProd, Model.Log log = null);

        Model.Product1 FindByProduct(long id, Model.Log log = null);
    }
}
