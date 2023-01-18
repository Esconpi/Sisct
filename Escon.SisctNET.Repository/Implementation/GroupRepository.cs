using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        private readonly ContextDataBase _context;

        public GroupRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
        
        public Group FindByDescription(string description, Log log = null)
        {
            var rst = _context.Groups.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
