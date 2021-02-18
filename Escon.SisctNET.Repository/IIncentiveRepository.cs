using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IIncentiveRepository : IRepository<Model.Incentive>
    {
        List<Model.Incentive> FindByCompany(int company, Model.Log log = null);

        List<Model.Incentive> FindByPeriod(int days, Model.Log log = null);
    }
}
