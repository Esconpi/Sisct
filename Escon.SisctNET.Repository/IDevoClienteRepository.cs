using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IDevoClienteRepository : IRepository<Model.DevoCliente>
    {
        List<Model.DevoCliente> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
