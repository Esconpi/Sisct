using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct3Repository : IRepository<Model.Product3>
    {
        void Create(List<Model.Product3> products, Model.Log log = null);

        void Update(List<Model.Product3> products, Model.Log log = null);

        Model.Product3 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(long id, Model.Log log = null);

        Model.Product3 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product3> FindByGroup(long groupid, Model.Log log = null);

        List<Model.Product3> FindAllInDate2(DateTime dateProd, Model.Log log = null);

        List<Model.Product3> FindByAllGroup(Model.Log log = null);

        Model.Product3 FindByProduct(long id, Model.Log log = null);
    }
}
