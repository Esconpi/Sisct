using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface IEmailResponsibleService : IServiceBase<Model.EmailResponsible>
    {
        Task<List<Model.EmailResponsible>> GetByCompanyAsync(long companyId);
    }
}