using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface IStateService : IServiceBase<Model.State>
    {
        void Create(List<Model.State> states, Model.Log log = null);

        Model.State FindByUf(string uf, Model.Log log = null);

        Model.State FindByUf(List<State> states, DateTime data, string ufOrigem, string ufDestino, Model.Log log = null);
    }
}
