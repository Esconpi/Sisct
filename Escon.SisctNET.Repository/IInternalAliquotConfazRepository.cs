using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IInternalAliquotConfazRepository : IRepository<Model.InternalAliquotConfaz>
    {
        Model.InternalAliquotConfaz FindByAliquot(long stateId, long annexId, Model.Log log = null);

        List<Model.InternalAliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
