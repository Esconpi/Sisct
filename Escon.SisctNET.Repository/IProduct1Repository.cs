using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct1Repository : IRepository<Model.Product1>
    {
        Model.Product1 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product1> FindByGroup(long groupid, Model.Log log = null);

        List<Model.Product1> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.Product1> FindByAllGroup(Model.Log log = null);

        Model.Product1 FindByProduct(long id, Model.Log log = null);
    }
}
