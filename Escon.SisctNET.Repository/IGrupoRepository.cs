using Escon.SisctNET.Model;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IGrupoRepository : IRepository<Model.Grupo>
    {
        void Create(List<Model.Grupo> grupos, Model.Log log = null);

        void Update(List<Model.Grupo> grupos, Model.Log log = null);

        List<Model.Grupo> FindByGrupos(int taxid, Log log = null);
    }
}
