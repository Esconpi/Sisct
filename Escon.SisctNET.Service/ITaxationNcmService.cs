using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxationNcmService : IServiceBase<Model.TaxationNcm>
    {
        List<Model.TaxationNcm> FindByPeriod(List<Model.TaxationNcm> taxationNcms, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.TaxationNcm> FindByCompany(string company, Model.Log log = null);

        List<Model.TaxationNcm> FindByCompany(long company, Model.Log log = null);

        List<Model.TaxationNcm> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.TaxationNcm> FindMono(long typeCompany, Model.Log log = null);

        List<Model.TaxationNcm> FindAllInDate(List<TaxationNcm> ncms, DateTime dateProd, Model.Log log = null);

        List<Model.TaxationNcm> FindByCompany(long company, string year, string month, Model.Log log = null);

        List<Model.TaxationNcm> FindByGeneral(Model.Log log = null);

        void Create(List<Model.TaxationNcm> taxationNcms, Model.Log log = null);

        void Update(List<Model.TaxationNcm> taxationNcms, Model.Log log = null);
    }
}
