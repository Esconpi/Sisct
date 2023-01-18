using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service.Implementation
{
    public class EmailResponsibleService : IEmailResponsibleService
    {
        private readonly IEmailResponsibleRepository _repository;
        public EmailResponsibleService(IEmailResponsibleRepository repository)
        {
            _repository = repository;
        }

        public EmailResponsible Create(EmailResponsible entity, Log log) => _repository.Create(entity, log);

        public List<EmailResponsible> Create(List<EmailResponsible> entities, Log log) => _repository.Create(entities, log);

        public void Delete(long id, Log log) => _repository.Delete(id, log);

        public List<EmailResponsible> FindAll(Log log) => _repository.FindAll(log);

        public List<EmailResponsible> FindAll(int page, int countrow, Log log) => _repository.FindAll(page, countrow, log);

        public EmailResponsible FindById(long id, Log log) => _repository.FindById(id, log);

        public Task<List<EmailResponsible>> GetByCompanyAsync(long companyId) => _repository.GetByCompanyAsync(companyId);

        public EmailResponsible Update(EmailResponsible entity, Log log) => _repository.Update(entity, log);

        public List<EmailResponsible> Update(List<EmailResponsible> entities, Log log) => _repository.Update(entities, log);
    }
}