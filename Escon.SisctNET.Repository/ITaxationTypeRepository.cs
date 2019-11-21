using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxationTypeRepository : IRepository<Model.TaxationType>
    {
        void Create(List<Model.TaxationType> taxationTypes, Model.Log log = null);

        Model.TaxationType FindByDescription(string description, Model.Log log = null);
    }
}
