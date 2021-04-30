using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Notification FindByCurrentMonth(long companyid, string month, string year, Log log = null);

        List<Notification> FindByYear(long companyid, string year, Log log = null);
    }
}
