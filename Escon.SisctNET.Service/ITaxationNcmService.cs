﻿
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ITaxationNcmService : IServiceBase<Model.TaxationNcm>
    {
        List<Model.TaxationNcm> FindAllInDate(DateTime dateProd, Model.Log log = null);
    }
}
