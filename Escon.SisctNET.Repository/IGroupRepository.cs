using System.Collections.Generic;


namespace Escon.SisctNET.Repository
{
    public interface IGroupRepository : IRepository<Model.Group>
    {
        void Create(List<Model.Group> groups, Model.Log log = null);

        Model.Group FindByDescription(string description, Model.Log log = null);

    }
}
