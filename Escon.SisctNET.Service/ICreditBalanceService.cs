using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service
{
    public interface ICreditBalanceService : IServiceBase<CreditBalance>
    {
        CreditBalance FindByLastMonth(int companyid, string month, string year, Log log = null);

        CreditBalance FindByCurrentMonth(int companyid, string month, string year, Log log = null);
    }
}
