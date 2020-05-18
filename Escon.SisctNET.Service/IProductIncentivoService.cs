using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface IProductIncentivoService : IServiceBase<Model.ProductIncentivo>
    {
        List<Model.ProductIncentivo> FindByProducts(int id, string year, string month, Model.Log log = null);

        Model.ProductIncentivo FindByProduct(int company, string code, string ncm,string cest, Model.Log log = null);

        List<Model.ProductIncentivo> FindByAllProducts(int company, Model.Log log = null);

        List<Model.ProductIncentivo> FindByDate(int company, DateTime date, Model.Log log = null);

    }
}
