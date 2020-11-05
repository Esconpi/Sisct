using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CsosnRepository : Repository<Model.Csosn>,ICsosnRepository
    {
        private readonly ContextDataBase _context;

        public CsosnRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }
    }
}
