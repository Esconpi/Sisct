using Escon.SisctNET.Model;

namespace Escon.SisctNET.Service
{
    public interface ICreditBalanceService : IServiceBase<CreditBalance>
    {
        CreditBalance FindByLastMonth(long companyid, string month, string year, Log log = null);

        CreditBalance FindByCurrentMonth(long companyid, string month, string year, Log log = null);
    }
}
