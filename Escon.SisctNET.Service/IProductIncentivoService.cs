using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductIncentivoService : IServiceBase<Model.ProductIncentivo>
    {
        List<Model.ProductIncentivo> FindByProducts(long id, string year, string month, Model.Log log = null);

        Model.ProductIncentivo FindByProduct(long company, string code, string ncm,string cest, Model.Log log = null);

        List<Model.ProductIncentivo> FindByAllProducts(long company, Model.Log log = null);

        List<Model.ProductIncentivo> FindByDate(long company, DateTime date, Model.Log log = null);

        List<Model.ProductIncentivo> FindByAllProducts(string company, Model.Log log = null);

        List<Model.ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, DateTime date, Model.Log log = null);

        List<Model.ProductIncentivo> FindByProducts(List<Model.ProductIncentivo> productIncentivos, string ncmRaiz, Model.Log log = null);
    }
}
