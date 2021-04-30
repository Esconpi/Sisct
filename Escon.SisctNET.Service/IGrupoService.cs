using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IGrupoService : IServiceBase<Model.Grupo>
    {
        void Create(List<Model.Grupo> grupos, Model.Log log = null);

        void Update(List<Model.Grupo> grupos, Model.Log log = null);

        List<Model.Grupo> FindByGrupos(long taxid, Log log = null);
    }
}
