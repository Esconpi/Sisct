﻿using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ICompanyCfopService : IServiceBase<Model.CompanyCfop>
    {
        void Create(List<Model.CompanyCfop> cfopCompanies, Model.Log log = null);

        List<Model.CompanyCfop> FindByCompany(int companyId, Log log = null);

        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopDevoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopVendaST(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopCompra(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopTransferencia(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoVenda(string company, Log log = null);

        List<Model.CompanyCfop> FindByCfopBonificacaoCompra(string company, Log log = null);
    }
}
