using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CfopRepository : Repository<Cfop>, ICfopRepository
    {
        private readonly ContextDataBase _context;

        public CfopRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Cfop> FindByCfopBonificacaoCompra(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Bonificação de Compra")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopBonificacaoVenda(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Bonificação de Venda")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopCompra(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Compra")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopCompraIM(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Compra Imobilizado")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopCompraST(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Compra ST")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopDevoCompra(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Devolução de Compra")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopDevoCompraST(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Devolução de Compra ST")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopDevoVenda(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Devolução de Venda")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopDevoVendaST(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Devolução de Venda ST")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopOutraEntrada(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Outra Entrada")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopOutraSaida(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Outra Saída")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopPerda(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Perda")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopTransferencia(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Transferência")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopTransferenciaST(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Transferência ST")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopVenda(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Venda")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopVendaIM(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Venda Imobilizado")).ToList();
            AddLog(log);
            return result;
        }

        public List<Cfop> FindByCfopVendaST(List<Cfop> cfops, Log log = null)
        {
            var result = cfops.Where(_ => _.Active.Equals(true) && _.CfopType.Name.Equals("Venda ST")).ToList();
            AddLog(log);
            return result;
        }

        public Cfop FindByCode(string code, Log log = null)
        {
            var rst = _context.Cfops.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Cfop> FindByType(Log log = null)
        {
            var rst = _context.Cfops.Include(_ => _.CfopType).ToList();
            AddLog(log);
            return rst;
        }
    }
}
