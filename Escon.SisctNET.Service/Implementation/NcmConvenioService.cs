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

        public void Delete(long id, Log log)
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

        public List<NcmConvenio> FindByAnnex(long annexId, Log log = null)
        {
            return _repository.FindByAnnex(annexId, log);
        }

        public NcmConvenio FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null)
        {
            return _repository.FindByNcmAnnex(annexId, log);
        }

        public bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, string cest, Company comp, Log log = null)
        {
            return _repository.FindByNcmAnnex(ncms, ncm, cest, comp, log);
        }

        public bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, Log log = null)
        {
            return _repository.FindByNcmAnnex(ncms, ncm, log);
        }

        public bool FindByNcmAnnex(long Annex, string ncm, Log log = null)
        {
            return _repository.FindByNcmAnnex(Annex, ncm, log);
        }

        public NcmConvenio Update(NcmConvenio entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
