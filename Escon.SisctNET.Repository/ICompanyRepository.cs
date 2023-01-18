﻿using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface ICompanyRepository : IRepository<Model.Company>
    {
        Model.Company FindByCode(string code, Model.Log log = null);

        Model.Company FindByDocument(string document, Model.Log log = null);

        List<Model.Company> FindByCompanies(Model.Log log = null);

        List<Model.Company> FindByCompanies(string company,Model.Log log = null);

        Task<List<Company>> ListAllActiveAsync(Log log);

    }
}