using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IInternalAliquotConfazRepository : IRepository<Model.InternalAliquotConfaz>
    {
        List<Model.InternalAliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
