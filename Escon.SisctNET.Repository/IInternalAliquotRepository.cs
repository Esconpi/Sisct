using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IInternalAliquotRepository : IRepository<Model.InternalAliquot>
    {
        List<Model.InternalAliquot> FindByAllState(Model.Log log = null);
    }
}
