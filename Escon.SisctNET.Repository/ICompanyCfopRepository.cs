using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ICompanyCfopRepository : IRepository<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferencia(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferenciaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraEntrada(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraSaida(string company, Log log = null);
    }
}
