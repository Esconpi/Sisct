using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct2Repository : IRepository<Model.Product2>
    {
        Model.Product2 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(long id, Model.Log log = null);

        Model.Product2 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product2> FindByGroup(long groupid, Model.Log log = null);

        List<Model.Product2> FindAllInDate2(DateTime dateProd, Model.Log log = null);

        List<Model.Product2> FindByAllGroup(Model.Log log = null);

        Model.Product2 FindByProduct(long id, Model.Log log = null);
    }
}

