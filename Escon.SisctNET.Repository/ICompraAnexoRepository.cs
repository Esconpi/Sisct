using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository
{
    public interface ICompraAnexoRepository : IRepository<Model.CompraAnexo>
    {
        void Create(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        void Update(List<Model.CompraAnexo> compraAnexos, Model.Log log = null);

        List<Model.CompraAnexo> FindByComprasTax(int taxAnexo, Model.Log log = null);
    }
}
