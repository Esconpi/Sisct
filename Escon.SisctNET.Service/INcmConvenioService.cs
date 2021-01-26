﻿using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INcmConvenioService : IServiceBase<Model.NcmConvenio>
    {
        List<string> FindByAnnex(int annexId, Log log = null);

        List<NcmConvenio> FindByNcmAnnex(int annexId, Log log = null);

        bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, Model.Log log = null);
    }
}
