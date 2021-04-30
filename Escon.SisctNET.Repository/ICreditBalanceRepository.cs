using Escon.SisctNET.Model;

namespace Escon.SisctNET.Repository
{
    public interface ICreditBalanceRepository : IRepository<CreditBalance>
    {
        CreditBalance FindByLastMonth(long companyid, string month, string year, Log log = null);

        CreditBalance FindByCurrentMonth(long companyid, string month, string year, Log log = null);
    }
}
