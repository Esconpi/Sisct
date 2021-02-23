using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICstRepository : IRepository<Model.Cst>
    {
        List<Model.Cst> FindByIdent(bool identicador, Model.Log log = null);
    }
}
