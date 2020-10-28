using Escon.SisctNET.Model;

namespace Escon.SisctNET.Repository
{
    public interface ICreditBalanceRepository : IRepository<CreditBalance>
    {
        CreditBalance FindByLastMonth(int companyid, string month, string year, Log log = null);

        CreditBalance FindByCurrentMonth(int companyid, string month, string year, Log log = null);
    }
}
