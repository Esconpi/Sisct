using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompraAnexoRepository : IRepository<Model.CompraAnexo>
    {
        void Create(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        void Update(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        List<Model.CompraAnexo> FindByComprasTax(long taxAnexo, Model.Log log = null);
    }
}
