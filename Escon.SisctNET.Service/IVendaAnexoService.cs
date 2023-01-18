using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IVendaAnexoService : IServiceBase<Model.VendaAnexo>
    {
        List<Model.VendaAnexo> FindByVendasTax(long taxAnexo, Model.Log log = null);
    }
}
