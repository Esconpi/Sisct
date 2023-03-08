using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductRepository : IRepository<Model.Product>
    {
        Model.Product FindByProduct(long id, Model.Log log = null);

        Model.Product FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product> FindByGroup(long groupid, Model.Log log = null);

        List<Model.Product> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.Product> FindByAllGroup(Model.Log log = null);
    }
}
