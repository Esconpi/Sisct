using System.Collections.Generic;
﻿using System;

namespace Escon.SisctNET.Repository
{
    public interface ITaxationRepository : IRepository<Model.Taxation>
    {
        Model.Taxation FindByCode(string code, string cest, DateTime data, Model.Log log = null);

        Model.Taxation FindByCode2(string code2, Model.Log log = null);

        List<Model.Taxation> FindByCompany(int companyId, Model.Log log = null);

        Model.Taxation FindByNcm(string code, string cest, Model.Log log = null);
    }
}
