using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INcmConvenioRepository : IRepository<Model.NcmConvenio>
    {
        List<string> FindByAnnex(int annexId, Log log = null);
    }
}
