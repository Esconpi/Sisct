using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AnnexRepository : Repository<Model.Annex>, IAnnexRepository
    {
        private readonly ContextDataBase _context;

        public AnnexRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
