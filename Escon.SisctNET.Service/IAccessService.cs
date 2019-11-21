using System.Collections.Generic;


namespace Escon.SisctNET.Service
{
    public interface IAccessService : IServiceBase<Model.Access>
    {
        List<Model.Access> FindByFunctionalityId(int functionalityId, Model.Log log = null);

        List<Model.Access> FindByProfileId(int profileId, Model.Log log = null);

        List<Model.Access> FindByActive(int profileId, Model.Log log = null);

    }
}

