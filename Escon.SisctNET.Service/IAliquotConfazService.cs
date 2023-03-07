using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAliquotConfazService : IServiceBase<Model.AliquotConfaz>
    {
        Model.AliquotConfaz FindByAliquot(long stateOrigemId, long stateDestinoId, long annexId, Model.Log log = null);

        AliquotConfaz FindByUf(List<AliquotConfaz> aliquotConfazs, DateTime data, string ufOrigem, string ufDestino, long annexId, Log log = null);

        List<Model.AliquotConfaz> FindByAllState(Model.Log log = null);
    }
}
