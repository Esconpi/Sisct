using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IPersonService : IServiceBase<Model.Person>
    {
        List<Model.Person> FindByProfileId(int profileId, Model.Log log = null);
    }
}
