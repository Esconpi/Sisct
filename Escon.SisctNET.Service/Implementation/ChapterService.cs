using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class ChapterService : IChapterService
    {
        private readonly IChapterRepository _repository;

        public ChapterService(IChapterRepository repository)
        {
            _repository = repository;
        }

        public Chapter Create(Chapter entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<Chapter> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Chapter> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public Chapter FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Chapter Update(Chapter entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
