using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAliquotService : IServiceBase<Model.Aliquot>
    {
        Model.Aliquot FindByUf(string uf, Model.Log log = null);

        Model.Aliquot FindByUf(string ufOrigem, string ufDestino, DateTime data, Model.Log log = null);

        Model.Aliquot FindByUf(List<Aliquot> aliquots, DateTime data, string ufOrigem, string ufDestino, Model.Log log = null);

        List<Model.Aliquot> FindByAllState(Model.Log log = null);

        Model.Aliquot FindByAliquot(long stateOrigemId, long stateDestinoId, Model.Log log = null);
    }
}
