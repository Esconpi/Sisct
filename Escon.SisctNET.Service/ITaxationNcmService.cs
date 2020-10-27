﻿using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxationNcmService : IServiceBase<Model.TaxationNcm>
    {
        List<Model.TaxationNcm> FindByCompany(string company, Model.Log log = null);

        List<Model.TaxationNcm> FindByCompany(int company, Model.Log log = null);

        List<Model.TaxationNcm> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.TaxationNcm> FindMono(int typeCompany, Model.Log log = null);

        List<Model.TaxationNcm> FindAllInDate(List<TaxationNcm> ncms, DateTime dateProd, Model.Log log = null);

        void Create(List<Model.TaxationNcm> taxationNcms, Model.Log log = null);

        void Update(List<Model.TaxationNcm> taxationNcms, Model.Log log = null);
    }
}
