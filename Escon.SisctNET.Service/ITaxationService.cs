using System.Collections.Generic;
using System;

namespace Escon.SisctNET.Service
{
    public interface ITaxationService : IServiceBase<Model.Taxation>
    {
        Model.Taxation FindByCode(string code, DateTime data, Model.Log log = null);

        Model.Taxation FindByCode2(string code2, Model.Log log = null);

        List<Model.Taxation> FindByCompany(int companyId, Model.Log log = null);
    }
}
