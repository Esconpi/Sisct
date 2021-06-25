using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INcmConvenioService : IServiceBase<Model.NcmConvenio>
    {
        List<string> FindByAnnex(long annexId, Log log = null);

        List<NcmConvenio> FindByNcmAnnex(long annexId, Log log = null);

        bool FindByNcmAnnex(List<NcmConvenio> ncms, string ncm, string cest, Model.Log log = null);
    }
}
