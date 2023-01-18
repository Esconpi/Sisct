using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IVendaAnexoRepository : IRepository<Model.VendaAnexo>
    {
        List<Model.VendaAnexo> FindByVendasTax(long taxAnexo, Model.Log log = null);
    }
}
