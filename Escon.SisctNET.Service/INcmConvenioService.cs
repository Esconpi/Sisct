using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INcmConvenioService : IServiceBase<Model.NcmConvenio>
    {
        bool FindByNcmExists(List<NcmConvenio> ncms, string ncm, Model.Log log = null);

        bool FindByNcmExists(List<NcmConvenio> ncms, string ncm, string cest, Company comp, Model.Log log = null);

        NcmConvenio FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, Model.Log log = null);

        NcmConvenio FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, string cest, Company comp, Model.Log log = null);

        List<NcmConvenio> FindByAnnex(Log log = null);

        List<NcmConvenio> FindByAnnex(long annexId, Log log = null);

        List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null);

        List<NcmConvenio> FindAllInDate(List<NcmConvenio> ncms, DateTime dateNCM, Log log = null);
    }
}
