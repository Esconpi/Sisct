using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface IEmailResponsibleRepository : IRepository<Model.EmailResponsible>
    {
        Task<List<Model.EmailResponsible>> GetByCompanyAsync(long companyId);
    }
}