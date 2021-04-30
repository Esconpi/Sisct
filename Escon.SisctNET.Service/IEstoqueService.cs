using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IEstoqueService : IServiceBase<Model.Estoque>
    {
        List<Model.Estoque> FindByCompany(long company, Model.Log log = null);
    }
}
