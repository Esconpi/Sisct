using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAliquotService : IServiceBase<Model.Aliquot>
    {
        void Create(List<Model.Aliquot> states, Model.Log log = null);

        Model.Aliquot FindByUf(string uf, Model.Log log = null);

        Model.Aliquot FindByUf(List<Aliquot> states, DateTime data, string ufOrigem, string ufDestino, Model.Log log = null);
    }
}
