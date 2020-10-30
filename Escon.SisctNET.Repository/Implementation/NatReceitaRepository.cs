using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NatReceitaRepository : Repository<Model.NatReceita>, INatReceitaRepository
    {
        private readonly ContextDataBase _context;

        public NatReceitaRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }
    }
}
