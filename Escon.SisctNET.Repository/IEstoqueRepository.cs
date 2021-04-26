using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IEstoqueRepository : IRepository<Model.Estoque>
    {
        List<Model.Estoque> FindByCompany(int company, Model.Log log = null);
    }
}
