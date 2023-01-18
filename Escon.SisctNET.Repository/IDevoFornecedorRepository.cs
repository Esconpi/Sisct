using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IDevoFornecedorRepository : IRepository<Model.DevoFornecedor>
    {
        List<Model.DevoFornecedor> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
