using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface IDarService : IServiceBase<Dar>
    {
        Task<List<Dar>> FindAllAsync(Log log);
    }
}
