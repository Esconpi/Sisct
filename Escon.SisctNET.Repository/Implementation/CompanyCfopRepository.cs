using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CompanyCfopRepository : Repository<Model.CompanyCfop>, ICompanyCfopRepository
    {
        private readonly ContextDataBase _context;

        public CompanyCfopRepository(ContextDataBase context, IConfiguration configuration) 
            : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<CompanyCfop> cfopCompanies, Log log = null)
        {
            foreach (var c in cfopCompanies)
            {
                _context.CompanyCfops.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<CompanyCfop> FindByCfopBonificacaoCompra(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(8)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(4)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompra(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(6)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(9)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoCompra(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0,8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(3)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoVenda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(7)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraEntrada(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(12)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraSaida(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(13)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopTransferencia(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(2)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopTransferenciaST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(10)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVenda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(1)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVendaST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals(5)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCompany(int companyId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId));
            AddLog(log);
            return result.ToList();
        }

        public CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.CfopId.Equals(cfopId)).FirstOrDefault();
            AddLog(log);
            return result;

        }

    }
}