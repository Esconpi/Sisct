using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICstService : IServiceBase<Model.Cst>
    {
        List<Model.Cst> FindByIdent(bool identicador, Model.Log log = null);
    }

}
