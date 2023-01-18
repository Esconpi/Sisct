using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IDevoClienteService : IServiceBase<Model.DevoCliente>
    {
        List<Model.DevoCliente> FindByDevoTax(long taxAnexo, Model.Log log = null);
    }
}
