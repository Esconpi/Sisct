using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IVendaAnexoRepository : IRepository<Model.VendaAnexo>
    {
        void Create(List<Model.VendaAnexo> vendaAnexos, Model.Log log = null);

        void Update(List<Model.VendaAnexo> vendaAnexos, Model.Log log = null);

        List<Model.VendaAnexo> FindByVendasTax(int taxAnexo, Model.Log log = null);
    }
}
