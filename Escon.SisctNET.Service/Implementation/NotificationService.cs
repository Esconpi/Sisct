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

        public void Delete(int id, Log log)
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

        public Notification FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Notification Update(Notification entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
