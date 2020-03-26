﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface IProductIncentivoService : IServiceBase<Model.ProductIncentivo>
    {
        List<Model.ProductIncentivo> FindByProducts(int id, string year, string month, Model.Log log = null);

        Model.ProductIncentivo FindByProduct(int company, string ncm, string code, Model.Log log = null);
    }
}
