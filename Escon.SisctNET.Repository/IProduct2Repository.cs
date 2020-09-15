using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProduct2Repository : IRepository<Model.Product2>
    {
        void Create(List<Model.Product2> products, Model.Log log = null);

        void Update(List<Model.Product2> products, Model.Log log = null);

        Model.Product2 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(int id, Model.Log log = null);

        Model.Product2 FindByProduct(string code, int grupoId, Model.Log log = null);
    }
}

