using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class CfopService : ICfopService
    {
        private readonly ICfopRepository _repository;

        public CfopService(ICfopRepository repository)
        {
            _repository = repository;
        }
     
        public Cfop Create(Cfop entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Cfop> Create(List<Cfop> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Cfop> entities, Log log)
        {
            _repository.Delete(entities, log);
        }

        public List<Cfop> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Cfop> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Cfop> FindByCfopBonificacaoCompra(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopBonificacaoCompra(cfops, log);
        }

        public List<Cfop> FindByCfopBonificacaoVenda(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopBonificacaoVenda(cfops, log);
        }

        public List<Cfop> FindByCfopCompra(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopCompra(cfops, log);
        }

        public List<Cfop> FindByCfopCompraIM(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopCompraIM(cfops, log);
        }

        public List<Cfop> FindByCfopCompraST(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopCompraST(cfops, log);
        }

        public List<Cfop> FindByCfopDevoCompra(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopDevoCompra(cfops, log);
        }

        public List<Cfop> FindByCfopDevoCompraST(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopDevoCompraST(cfops, log);
        }

        public List<Cfop> FindByCfopDevoVenda(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopDevoVenda(cfops, log);
        }

        public List<Cfop> FindByCfopDevoVendaST(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopDevoVendaST(cfops, log);
        }

        public List<Cfop> FindByCfopOutraEntrada(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopOutraEntrada(cfops, log);
        }

        public List<Cfop> FindByCfopOutraSaida(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopOutraSaida(cfops, log);
        }

        public List<Cfop> FindByCfopPerda(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopPerda(cfops, log);
        }

        public List<Cfop> FindByCfopTransferencia(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopTransferencia(cfops, log);
        }

        public List<Cfop> FindByCfopTransferenciaST(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopTransferenciaST(cfops, log);
        }

        public List<Cfop> FindByCfopVenda(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopVenda(cfops, log);
        }

        public List<Cfop> FindByCfopVendaIM(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopVendaIM(cfops, log);
        }

        public List<Cfop> FindByCfopVendaST(List<Cfop> cfops, Log log = null)
        {
            return _repository.FindByCfopVendaST(cfops, log);
        }

        public Cfop FindByCode(string code, Log log = null)
        {
            return _repository.FindByCode(code);
        }

        public Cfop FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public List<Cfop> FindByType(Log log = null)
        {
            return _repository.FindByType(log);
        }

        public Cfop Update(Cfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Cfop> Update(List<Cfop> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
