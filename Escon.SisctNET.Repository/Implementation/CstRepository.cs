using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CstRepository : Repository<Model.Cst>, ICstRepository
    {
        private readonly ContextDataBase _context;

        public CstRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
