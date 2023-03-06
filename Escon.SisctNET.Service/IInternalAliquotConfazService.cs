using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IInternalAliquotConfazService : IServiceBase<Model.InternalAliquotConfaz>
    {
        List<Model.InternalAliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
