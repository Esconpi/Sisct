using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IIncentiveService : IServiceBase<Model.Incentive>
    {
        List<Model.Incentive> FindByCompany(long company, Model.Log log = null);

        List<Model.Incentive> FindByPeriod(int days, Model.Log log = null);
    }
}
