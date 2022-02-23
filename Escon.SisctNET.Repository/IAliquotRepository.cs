using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAliquotRepository : IRepository<Model.Aliquot>
    {
        void Create(List<Model.Aliquot> states, Model.Log log = null);

        Model.Aliquot FindByUf(string uf, Model.Log log = null);

        Model.Aliquot FindByUf(List<Aliquot> states, DateTime data, string ufOrigem, string ufDestino, Model.Log log = null);

        List<Model.Aliquot> FindByAllState(Model.Log log = null);
    }
}