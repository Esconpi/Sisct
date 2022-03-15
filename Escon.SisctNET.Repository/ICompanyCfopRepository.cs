using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompanyCfopRepository : IRepository<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        Model.CompanyCfop FindByCompanyCfop(long companyId, ulong cfopId, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(long companyId, Log log = null);

        List<Model.CompanyCfop> FindByCompany(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompra(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompraST(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaST(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompra(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraST(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoCompra(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVenda(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVendaST(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopVenda(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoVenda(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferencia(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferenciaST(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraEntrada(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraSaida(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaIM(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraIM(List<Model.CompanyCfop> companyCfops, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraPerda(List<Model.CompanyCfop> companyCfops, Log log = null);
    }
}
