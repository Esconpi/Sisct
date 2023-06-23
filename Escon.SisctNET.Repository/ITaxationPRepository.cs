using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxationPRepository : IRepository<Model.TaxationP>
    {
        Model.TaxationP FindByNcm(string code, string cest, Model.Log log = null);

        Model.TaxationP FindByCode(List<TaxationP> taxations, string code, string cest, DateTime data, Model.Log log = null);

        List<Model.TaxationP> FindByCompany(long companyId, Model.Log log = null);

        List<Model.TaxationP> FindByCompanyActive(long companyId, Model.Log log = null);
    }
}
