using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IDevoFornecedorService : IServiceBase<Model.DevoFornecedor>
    {
        List<Model.DevoFornecedor> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
