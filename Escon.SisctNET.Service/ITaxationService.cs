using System.Collections.Generic;
using System;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service
{
    public interface ITaxationService : IServiceBase<Model.Taxation>
    {
        Model.Taxation FindByCode(string code, string cest, DateTime data, Model.Log log = null);

        List<Model.Taxation> FindByCompany(int companyId, Model.Log log = null);

        Model.Taxation FindByNcm(string code, string cest, Model.Log log = null);

        Model.Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Model.Log log = null);
    }
}
