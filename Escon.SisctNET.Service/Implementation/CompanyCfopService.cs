using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class CompanyCfopService : ICompanyCfopService
    {
        private readonly ICompanyCfopRepository _repository;

        public CompanyCfopService(ICompanyCfopRepository repository)
        {
            _repository = repository;
        }

        public CompanyCfop Create(CompanyCfop entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<CompanyCfop> Create(List<CompanyCfop> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public List<CompanyCfop> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<CompanyCfop> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<CompanyCfop> FindByCfopBonificacaoCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopBonificacaoCompra(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopBonificacaoVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopBonificacaoVenda(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopCompra(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopCompraIM(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopCompraIM(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopCompraPerda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopCompraPerda(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopCompraST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopCompraST(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopDevoCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopDevoCompra(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopDevoCompraST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopDevoCompraST(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopDevoVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopDevoVenda(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopDevoVendaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopDevoVendaST(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopOutraEntrada(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopOutraEntrada(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopOutraSaida(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopOutraSaida(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopTransferencia(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopTransferencia(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopTransferenciaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopTransferenciaST(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopVenda(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopVendaIM(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopVendaIM(companyCfops, log);
        }

        public List<CompanyCfop> FindByCfopVendaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            return _repository.FindByCfopVendaST(companyCfops, log);
        }

        public List<CompanyCfop> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public List<CompanyCfop> FindByCompany(string company, Log log = null)
        {
            return _repository.FindByCompany(company, log);
        }

        public CompanyCfop FindByCompanyCfop(long companyId, ulong cfopId, Log log = null)
        {
            return _repository.FindByCompanyCfop(companyId, cfopId, log);
        }

        public CompanyCfop FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CompanyCfop Update(CompanyCfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<CompanyCfop> Update(List<CompanyCfop> entities, Log log)
        {
            return _repository.Update(entities, log);
        }
    }
}
