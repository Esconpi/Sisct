using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAliquotConfazRepository : IRepository<Model.AliquotConfaz>
    {
        Model.AliquotConfaz FindByAliquot(long stateOrigemId, long stateDestinoId, long annexId, Model.Log log = null);

        List<Model.AliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
