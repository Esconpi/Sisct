using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ConfigurationRepository : Repository<Configuration>, IConfigurationRepository
    {
        private readonly ContextDataBase _context;

        public ConfigurationRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public Configuration FindByName(string name, Log log = null)
        {
            AddLog(log);
            return _context.Configurations.Where(_ => _.Name.Equals(name)).FirstOrDefault();
        }
    }
}
