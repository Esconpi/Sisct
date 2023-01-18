using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompraAnexoRepository : IRepository<Model.CompraAnexo>
    {
        List<Model.CompraAnexo> FindByComprasTax(long taxAnexo, Model.Log log = null);
    }
}
