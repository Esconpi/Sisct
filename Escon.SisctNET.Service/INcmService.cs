using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INcmService : IServiceBase<Model.Ncm>
    {
        void Create(List<Model.Ncm> ncms, Model.Log log = null);

        Model.Ncm FindByCode(string code, Model.Log log = null);
    }
}
