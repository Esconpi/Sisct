using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface ITaxAnexoService : IServiceBase<Model.TaxAnexo>
    {
        Model.TaxAnexo FindByMonth(int company, string mes, string ano, Model.Log log = null);
    }
}
