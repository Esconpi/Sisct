using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IInternalAliquotService : IServiceBase<Model.InternalAliquot>
    {
        List<Model.InternalAliquot> FindByAllState(Model.Log log = null);
    }
}
