using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ISuspensionRepository : IRepository<Model.Suspension>
    {
        List<Model.Suspension> FindByCompany(long company, Model.Log log = null);
    }
}
