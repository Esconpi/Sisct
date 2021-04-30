using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IVendaAnexoService : IServiceBase<Model.VendaAnexo>
    {
        void Create(List<Model.VendaAnexo> vendaAnexos, Model.Log log = null);

        void Update(List<Model.VendaAnexo> vendaAnexos, Model.Log log = null);

        List<Model.VendaAnexo> FindByVendasTax(long taxAnexo, Model.Log log = null);
    }
}
