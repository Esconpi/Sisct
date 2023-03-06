using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAliquotConfazRepository : IRepository<Model.AliquotConfaz>
    {
        List<Model.AliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
