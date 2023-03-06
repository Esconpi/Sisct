using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAliquotConfazService : IServiceBase<Model.AliquotConfaz>
    {
        Model.AliquotConfaz FindByAliquot(long stateOrigemId, long stateDestinoId, long annexId, Model.Log log = null);

        List<Model.AliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
