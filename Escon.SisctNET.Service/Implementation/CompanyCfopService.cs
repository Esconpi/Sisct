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

        public void Create(List<CompanyCfop> cfopCompanies, Log log = null)
        {
            _repository.Create(cfopCompanies, log);
        }

        public void Delete(int id, Log log)
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

        public List<CompanyCfop> FindByCfopBonificacaoCompra(string company, Log log = null)
        {
            return _repository.FindByCfopBonificacaoCompra(company, log);
        }

        public List<CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null)
        {
            return _repository.FindByCfopBonificacaoVenda(company, log);
        }

        public List<CompanyCfop> FindByCfopCompra(string company, Log log = null)
        {
            return _repository.FindByCfopCompra(company, log);
        }

        public List<CompanyCfop> FindByCfopCompraST(string company, Log log = null)
        {
            return _repository.FindByCfopCompraST(company, log);
        }

        public List<CompanyCfop> FindByCfopDevoCompra(string company, Log log = null)
        {
            return _repository.FindByCfopDevoCompra(company, log);
        }

        public List<CompanyCfop> FindByCfopDevoVenda(string company, Log log = null)
        {
            return _repository.FindByCfopDevoVenda(company, log);
        }

        public List<CompanyCfop> FindByCfopOutraEntrada(string company, Log log = null)
        {
            return _repository.FindByCfopOutraEntrada(company, log);
        }

        public List<CompanyCfop> FindByCfopOutraSaida(string company, Log log = null)
        {
            return _repository.FindByCfopOutraSaida(company, log);
        }

        public List<CompanyCfop> FindByCfopTransferencia(string company, Log log = null)
        {
            return _repository.FindByCfopTransferencia(company, log);
        }

        public List<CompanyCfop> FindByCfopTransferenciaST(string company, Log log = null)
        {
            return _repository.FindByCfopTransferenciaST(company, log);
        }

        public List<CompanyCfop> FindByCfopVenda(string company, Log log = null)
        {
            return _repository.FindByCfopVenda(company, log);
        }

        public List<CompanyCfop> FindByCfopVendaST(string company, Log log = null)
        {
            return _repository.FindByCfopVendaST(company, log);
        }

        public List<CompanyCfop> FindByCompany(int companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null)
        {
            return _repository.FindByCompanyCfop(companyId, cfopId, log);
        }

        public CompanyCfop FindById(int id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public CompanyCfop Update(CompanyCfop entity, Log log)
        {
            return _repository.Update(entity, log);
        }
    }
}
