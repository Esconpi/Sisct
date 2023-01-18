using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
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

        public async Task CreateRange(List<CompanyCfop> companyCfops, Log log = null)
        {
            _context.CompanyCfops.AddRange(companyCfops);
            await _context.SaveChangesAsync();
        }

        public List<CompanyCfop> FindByCfopBonificacaoCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)8))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopBonificacaoVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)4))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)6))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraIM(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)19))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraPerda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)20))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopCompraST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)9))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoCompra(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)3))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoCompraST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)16))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)7))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopDevoVendaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)17))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraEntrada(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)12))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopOutraSaida(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)13))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopTransferencia(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)2))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopTransferenciaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals(10))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVenda(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)1))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVendaIM(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)18))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCfopVendaST(List<Model.CompanyCfop> companyCfops, Log log = null)
        {
            var result = companyCfops
                .Where(_ => _.Active.Equals(true) && _.CfopTypeId.Equals((long)5))
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCompany(long companyId, Log log = null)
        {
            var result = _context.CompanyCfops
                .Where(_ => _.CompanyId.Equals(companyId))
                .Include(c => c.Cfop)
                .ToList();
            AddLog(log);
            return result;
        }

        public List<CompanyCfop> FindByCompany(string company, Log log = null)
        {
            var result = _context.CompanyCfops
               .Where(_ => _.Company.Document.Substring(0, 8).Equals(company.Substring(0, 8)))
               .Include(c => c.Cfop)
               .ToList();
            AddLog(log);
            return result;
        }

        public CompanyCfop FindByCompanyCfop(long companyId, ulong cfopId, Log log = null)
        {
            var result = _context.CompanyCfops
                .Where(_ => _.CompanyId.Equals(companyId) && _.CfopId.Equals(cfopId))
                .FirstOrDefault();
            AddLog(log);
            return result;

        }

    }
}