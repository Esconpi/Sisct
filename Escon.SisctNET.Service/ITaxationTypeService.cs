using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxationTypeService : IServiceBase<Model.TaxationType>
    {
        void Create(List<Model.TaxationType> taxationTypes, Model.Log log = null);

        Model.TaxationType FindByDescription(string description, Model.Log log = null);
    }
}
