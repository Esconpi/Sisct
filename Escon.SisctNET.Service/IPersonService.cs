using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IPersonService : IServiceBase<Model.Person>
    {
        List<Model.Person> FindByProfileId(long profileId, Model.Log log = null);
    }
}
