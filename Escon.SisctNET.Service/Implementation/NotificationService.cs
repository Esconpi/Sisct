using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public Notification Create(Notification entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Notification> Create(List<Notification> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Notification> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Notification> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Notification FindByCurrentMonth(long companyid, string month, string year, Log log = null)
        {
            return _repository.FindByCurrentMonth(companyid, month, year, log);
        }

        public Notification FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Notification> FindByYear(long companyid, string year, Log log = null)
        {
            return _repository.FindByYear(companyid, year, log);
        }

        public Notification Update(Notification entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Notification> Update(List<Notification> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
