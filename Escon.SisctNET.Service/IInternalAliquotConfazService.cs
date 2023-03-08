using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IInternalAliquotConfazService : IServiceBase<Model.InternalAliquotConfaz>
    {

        Model.InternalAliquotConfaz FindByAliquot(long stateId, long annexId, Model.Log log = null);

        Model.InternalAliquotConfaz FindByUf(List<InternalAliquotConfaz> internalAliquotConfazs, DateTime data, string uf, long annexId, Model.Log log = null);

        List<Model.InternalAliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
