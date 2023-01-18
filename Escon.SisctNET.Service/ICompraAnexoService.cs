using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompraAnexoService : IServiceBase<Model.CompraAnexo>
    {
        List<Model.CompraAnexo> FindByComprasTax(long taxAnexo, Model.Log log = null);
    }
}
