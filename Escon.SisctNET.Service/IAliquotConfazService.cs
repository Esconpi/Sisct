using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAliquotConfazService : IServiceBase<Model.AliquotConfaz>
    {
        List<Model.AliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
