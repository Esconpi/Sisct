using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IGroupService : IServiceBase<Model.Group>
    {
        void Create(List<Model.Group> groups, Model.Log log = null);

        Model.Group FindByDescription(string description, Model.Log log = null);
    }
}
