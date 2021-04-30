using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IPersonRepository : IRepository<Model.Person>
    {
        List<Model.Person> FindByProfileId(long profileId, Model.Log log = null);
    }
}