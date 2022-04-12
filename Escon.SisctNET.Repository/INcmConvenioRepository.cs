﻿using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INcmConvenioRepository : IRepository<Model.NcmConvenio>
    {
        List<NcmConvenio> FindByAnnex(long annexId, Log log = null);

        List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null);

        bool FindByNcmAnnex(long Annex, string ncm, Log log = null);

        bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, Model.Log log = null);

        bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, string cest, Company comp, Model.Log log = null);
    }
}
