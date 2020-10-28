using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductIncentivoService : IServiceBase<Model.ProductIncentivo>
    {
        List<Model.ProductIncentivo> FindByProducts(int id, string year, string month, Model.Log log = null);

        Model.ProductIncentivo FindByProduct(int company, string code, string ncm,string cest, Model.Log log = null);

        List<Model.ProductIncentivo> FindByAllProducts(int company, Model.Log log = null);

        List<Model.ProductIncentivo> FindByDate(int company, DateTime date, Model.Log log = null);

        List<Model.ProductIncentivo> FindByDate(List<ProductIncentivo> productIncentivos, int company, DateTime date, Model.Log log = null);

        void Create(List<Model.ProductIncentivo> productIncentivos, Model.Log log = null);

        void Update(List<Model.ProductIncentivo> productIncentivos, Model.Log log = null);
    }
}
