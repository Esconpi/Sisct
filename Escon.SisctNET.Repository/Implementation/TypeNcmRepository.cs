using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TypeNcmRepository : Repository<Model.TypeNcm>, ITypeNcmRepository
    {
        private readonly ContextDataBase _context;

        public TypeNcmRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
