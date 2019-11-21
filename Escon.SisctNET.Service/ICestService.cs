using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface ICestService : IServiceBase<Model.Cest>
    {
        void Create(List<Model.Cest> cests, Model.Log log = null);

        Model.Cest FindByCode(string code, Model.Log log = null);
    }
}
