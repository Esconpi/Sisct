﻿using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IAccessService : IServiceBase<Model.Access>
    {
        List<Model.Access> FindByFunctionalityId(long functionalityId, Model.Log log = null);

        List<Model.Access> FindByProfileId(long profileId, Model.Log log = null);

        List<Model.Access> FindByActive(long profileId, Model.Log log = null);

    }
}

