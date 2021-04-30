using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IDevoClienteRepository : IRepository<Model.DevoCliente>
    {
        void Create(List<Model.DevoCliente> devoClientes, Model.Log log = null);

        void Update(List<Model.DevoCliente> devoClientes, Model.Log log = null);

        List<Model.DevoCliente> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
