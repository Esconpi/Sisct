using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class StateRepository : Repository<Model.State>, IStateRepository
    {
        private readonly ContextDataBase _context;

        public StateRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
