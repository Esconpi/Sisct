﻿using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProduct2Service : IServiceBase<Model.Product2>
    {
        void Create(List<Model.Product2> products, Model.Log log = null);

        void Update(List<Model.Product2> products, Model.Log log = null);

        Model.Product2 FindByDescription(string description, Model.Log log = null);

        decimal FindByPrice(long id, Model.Log log = null);

        Model.Product2 FindByProduct(string code, long grupoId, Model.Log log = null);

        List<Model.Product2> FindByGroup(long groupid, Model.Log log = null);

        List<Model.Product2> FindAllInDate2(DateTime dateProd, Model.Log log = null);

        List<Model.Product2> FindByAllGroup(Model.Log log = null);

        Model.Product2 FindByProduct(long id, Model.Log log = null);
    }
}
