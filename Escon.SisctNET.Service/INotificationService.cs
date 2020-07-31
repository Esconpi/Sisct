using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INotificationService : IServiceBase<Notification>
    {
        Notification FindByCurrentMonth(int companyid, string month, string year, Log log = null);

        List<Notification> FindByYear(int companyid, string year, Log log = null);
    }
}
