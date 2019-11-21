using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IPersonRepository : IRepository<Model.Person>
    {
        List<Model.Person> FindByProfileId(int profileId, Model.Log log = null);
    }
}