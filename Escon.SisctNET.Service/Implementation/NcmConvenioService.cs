using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class NcmConvenioService : INcmConvenioService
    {

        private readonly INcmConvenioRepository _repository;

        public NcmConvenioService(INcmConvenioRepository repository)
        {
            _repository = repository;
        }

        public NcmConvenio Create(NcmConvenio entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public void Delete(int id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<NcmConvenio> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<NcmConvenio> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<string> FindByAnnex(int annexId, Log log = null)
        {
            return _repository.FindByAnnex(annexId, log);
        }

        public NcmConvenio FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<NcmConvenio> FindByNcmAnnex(int annexId, Log log = null)
        {
            return _repository.FindByNcmAnnex(annexId, log);
        }

        public NcmConvenio Update(NcmConvenio entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
