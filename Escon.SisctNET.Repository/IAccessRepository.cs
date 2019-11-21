using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IAccessRepository : IRepository<Model.Access>
    {
        List<Model.Access> FindByFunctionalityId(int functionalityId, Model.Log log = null);

        List<Model.Access> FindByProfileId(int profileId, Model.Log log = null);

        List<Model.Access> FindByActive(int profileID, Model.Log log = null);

    }
}

