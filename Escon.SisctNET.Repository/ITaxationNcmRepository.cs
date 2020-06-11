
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ITaxationNcmRepository : IRepository<Model.TaxationNcm>
    {
        List<Model.TaxationNcm> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.TaxationNcm> FindMono(int typeCompany, Model.Log log = null);
    }
}
