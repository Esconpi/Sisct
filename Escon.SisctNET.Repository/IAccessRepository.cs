using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAccessRepository : IRepository<Model.Access>
    {
        List<Model.Access> FindByFunctionalityId(long functionalityId, Model.Log log = null);

        List<Model.Access> FindByProfileId(long profileId, Model.Log log = null);

        List<Model.Access> FindByActive(long profileID, Model.Log log = null);

    }
}

