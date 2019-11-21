using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IStateRepository : IRepository<Model.State>
    {
        void Create(List<Model.State> states, Model.Log log = null);

        Model.State FindByUf(string uf, Model.Log log = null);
    }
}
