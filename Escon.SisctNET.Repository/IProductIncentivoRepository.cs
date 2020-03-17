using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository
{
    public interface IProductIncentivoRepository : IRepository<Model.ProductIncentivo>
    {
        List<Model.ProductIncentivo> FindByProducts(int id, string year, string month, Model.Log log = null);
    }
}
