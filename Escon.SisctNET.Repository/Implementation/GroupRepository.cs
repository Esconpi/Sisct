using System.Collections.Generic;
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

        public void Create(List<Group> groups, Log log = null)
        {
            foreach (var c in groups)
            {
                _context.Groups.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        
        public Group FindByDescription(string description, Log log = null)
        {
            var rst = _context.Groups.Where(_ => _.Description.Equals(description)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
