using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompraAnexoService : IServiceBase<Model.CompraAnexo>
    {
        void Create(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        void Update(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        List<Model.CompraAnexo> FindByComprasTax(long taxAnexo, Model.Log log = null);
    }
}
