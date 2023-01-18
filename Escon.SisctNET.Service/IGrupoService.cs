using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IGrupoService : IServiceBase<Model.Grupo>
    {
        List<Model.Grupo> FindByGrupos(long taxid, Log log = null);
    }
}
