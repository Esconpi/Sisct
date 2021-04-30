using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ISuspensionService : IServiceBase<Model.Suspension>
    {
        List<Model.Suspension> FindByCompany(long company, Model.Log log = null);
    }
}
