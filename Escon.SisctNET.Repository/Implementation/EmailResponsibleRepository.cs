using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class EmailResponsibleRepository : Repository<EmailResponsible>, IEmailResponsibleRepository
    {
        private readonly ContextDataBase _context;

        public EmailResponsibleRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public async Task<List<EmailResponsible>> GetByCompanyAsync(long companyId)
        {
            var result = await _context.Set<EmailResponsible>().Where(x => x.CompanyId.Equals(companyId)).ToListAsync();
            return result;
        }
    }
}