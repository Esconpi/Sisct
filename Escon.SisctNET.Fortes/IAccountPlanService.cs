using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface IAccountPlanService
    {
        List<Model.AccountPlan> GetAccountPlans(Model.Company company, string connectionString);
    }
}
