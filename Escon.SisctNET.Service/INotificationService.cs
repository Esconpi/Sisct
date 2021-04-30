using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INotificationService : IServiceBase<Notification>
    {
        Notification FindByCurrentMonth(long companyid, string month, string year, Log log = null);

        List<Notification> FindByYear(long companyid, string year, Log log = null);
    }
}
