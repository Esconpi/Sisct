using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICestRepository : IRepository<Model.Cest>
    {
        void Create(List<Model.Cest> cests, Model.Log log = null);

        Model.Cest FindByCode(string code, Model.Log log = null);
    }
}
