using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _repository;

        public AttachmentService(IAttachmentRepository repository)
        {
            _repository = repository;
        }

        public Attachment Create(Attachment entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Attachment> Create(List<Attachment> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Attachment> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Attachment> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Attachment FindByDescription(string description, Log log = null)
        {
            return _repository.FindByDescription(description);
        }

        public Attachment FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Attachment Update(Attachment entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Attachment> Update(List<Attachment> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
