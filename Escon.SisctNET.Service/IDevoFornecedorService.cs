using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IDevoFornecedorService : IServiceBase<Model.DevoFornecedor>
    {
        void Create(List<Model.DevoFornecedor> devoFornecedors, Model.Log log = null);

        void Update(List<Model.DevoFornecedor> devoFornecedors, Model.Log log = null);

        List<Model.DevoFornecedor> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
