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
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)8)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)4)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompra(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)6)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraIM(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)19)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraPerda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)20)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)9)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoCompra(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0,8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)3)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoCompraST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)16)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoVenda(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)7)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoVendaST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)17)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraEntrada(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)12)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraSaida(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)13)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopTransferencia(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)2)).ToList();
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
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)1)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVendaIM(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)18)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVendaST(string company, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)) && _.Active.Equals(true) && _.CfopTypeId.Equals((long)5)).ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCompany(long companyId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId));
            AddLog(log);
            return result.ToList();
        }

        public CompanyCfop FindByCompanyCfop(long companyId, ulong cfopId, Log log = null)
        {
            var result = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.CfopId.Equals(cfopId)).FirstOrDefault();
            AddLog(log);
            return result;

        }

    }
}