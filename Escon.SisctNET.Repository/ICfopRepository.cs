using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICfopRepository : IRepository<Model.Cfop>
    {
        Model.Cfop FindByCode(string code, Model.Log log = null);

        List<Model.Cfop> FindByType(Log log = null);

        List<Model.Cfop> FindByCfopDevoCompra(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopDevoCompraST(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopVendaST(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopCompra(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopCompraST(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopBonificacaoCompra(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopDevoVenda(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopDevoVendaST(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopVenda(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopBonificacaoVenda(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopTransferencia(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopTransferenciaST(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopOutraEntrada(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopOutraSaida(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopVendaIM(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopCompraIM(List<Model.Cfop> cfops, Log log = null);

        List<Model.Cfop> FindByCfopPerda(List<Model.Cfop> cfops, Log log = null);
    }
}
