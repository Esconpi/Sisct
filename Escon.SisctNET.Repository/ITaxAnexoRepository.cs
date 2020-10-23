using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository
{
    public interface ITaxAnexoRepository : IRepository<Model.TaxAnexo>
    {
        Model.TaxAnexo FindByMonth(int company, string mes, string ano, Model.Log log = null);
    }
}
