using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IInternalAliquotService : IServiceBase<Model.InternalAliquot>
    {
        Model.InternalAliquot FindByUf(List<InternalAliquot> aliquots, DateTime data, string uf, Model.Log log = null);

        List<Model.InternalAliquot> FindByAllState(Model.Log log = null);

        Model.InternalAliquot FindByAliquot(long stateId, Model.Log log = null);
    }
}
