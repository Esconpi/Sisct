using Escon.SisctNET.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository
{
    public interface IDarRepository : IRepository<Model.Dar>
    {
        Task<List<Dar>> FindAllAsync(Model.Log log);
    }
}
