﻿using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompanyCfopService : IServiceBase<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        Model.CompanyCfop FindByCompanyCfop(long companyId, ulong cfopId, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(long companyId, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompraST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVendaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferencia(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferenciaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraEntrada(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopOutraSaida(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaIM(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompraIM(string company, Log log = null);
    }
}
