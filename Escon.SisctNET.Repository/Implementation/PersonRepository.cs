using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        private readonly ContextDataBase _context;

        public PersonRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public List<Person> FindByProfileId(long profileId, Log log = null)
        {
            List<Person> result = _context.Persons.Where(_ => _.ProfileId.Equals(profileId)).ToList();
            AddLog(log);
            return result;
        }
    }
}
