﻿using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAccountPlanRepository : IRepository<Model.AccountPlan>
    {
        List<Model.AccountPlan> FindByCompanyId(long companyId, Model.Log log = null);

        List<Model.AccountPlan> FindByAccountTypeId(long id, Model.Log log = null);

        List<Model.AccountPlan> FindByCompanyActive(long companyId, Model.Log log = null);

        List<Model.AccountPlan> FindByCompanyActive(string code, Model.Log log = null);

    }
}
