﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Service
{
    public interface ICfopService : IServiceBase<Model.Cfop>
    {
        void Create(List<Model.Cfop> cfops, Model.Log log = null);

        Model.Cfop FindByCode(string code, Model.Log log = null);
    }
}
