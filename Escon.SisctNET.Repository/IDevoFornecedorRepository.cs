using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IDevoFornecedorRepository : IRepository<Model.DevoFornecedor>
    {
        void Create(List<Model.DevoFornecedor> devoFornecedors, Model.Log log = null);

        void Update(List<Model.DevoFornecedor> devoFornecedors, Model.Log log = null);

        List<Model.DevoFornecedor> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
