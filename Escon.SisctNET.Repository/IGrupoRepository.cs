using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IGrupoRepository : IRepository<Model.Grupo>
    {
        List<Model.Grupo> FindByGrupos(long taxid, Log log = null);
    }
}
