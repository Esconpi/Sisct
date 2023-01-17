using System.Collections.Generic;
using System;
using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service
{
    public interface ITaxationService : IServiceBase<Model.Taxation>
    {
        void Update(List<Model.Taxation> taxations, Model.Log log = null);

        Model.Taxation FindByCode(string code, string cest, DateTime data, Model.Log log = null);

        Model.Taxation FindByNcm(string code, string cest, Model.Log log = null);

        Model.Taxation FindByCode(List<Taxation> taxations, string code, string cest, DateTime data, Model.Log log = null);

        List<Model.Taxation> FindByCompany(long companyId, Model.Log log = null);

        List<Model.Taxation> FindByCompanyActive(long companyId, Model.Log log = null);
    }
}
