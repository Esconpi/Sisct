using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AccessRepository : Repository<Access>, IAccessRepository
    {
        private readonly ContextDataBase _context;

        public AccessRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public List<Access> FindByFunctionalityId(long functionalityId, Log log = null)
        {
            var result = _context.Accesses.Where(_ => _.FunctionalityId.Equals(functionalityId)).ToList();
            AddLog(log);
            return result;
        }

        public List<Access> FindByProfileId(long profileId, Log log = null)
        {
            var result = _context.Accesses
                .Where(_ => _.ProfileId.Equals(profileId))
                .Include(p => p.Profile)
                .Include(f => f.Functionality)
                .ToList();
            AddLog(log);
            return result;
        }

        public List<Access> FindByActive(long profileId, Log log = null)
        {
            var result = _context.Accesses.Where(_ => _.ProfileId.Equals(profileId) && _.Active.Equals(true)).ToList();
            AddLog(log);
            return result;
        }
    }
}
