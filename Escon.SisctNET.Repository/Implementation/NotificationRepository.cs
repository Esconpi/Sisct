using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ContextDataBase _context;

        public NotificationRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Notification FindByCurrentMonth(int companyid, string month, string year, Log log = null)
        {
            AddLog(log);
            return _context.Notifications.Where(_ => _.CompanyId.Equals(companyid) && _.MesRef.Equals(month) && _.AnoRef.Equals(year)).FirstOrDefault();
        }

        public List<Notification> FindByYear(int companyid, string year, Log log = null)
        {
            AddLog(log);
            return _context.Notifications.Where(_ => _.CompanyId.Equals(companyid) && _.AnoRef.Equals(year)).ToList();
        }
    }
}
