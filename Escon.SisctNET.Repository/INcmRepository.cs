using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INcmRepository : IRepository<Model.Ncm>
    {
        void Create(List<Model.Ncm> ncms, Model.Log log = null);

        Model.Ncm FindByCode(string code, Model.Log log = null);
    }
}
