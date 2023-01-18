using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface INoteService : IServiceBase<Model.Note>
    {
        Task CreateRange(List<Model.Note> notes, Model.Log log = null);

        Task UpdateRange(List<Model.Note> notes, Model.Log log = null);

        Task DeleteRange(List<Model.Note> notes, Model.Log log = null);

        Model.Note FindByNote(string chave, Model.Log log = null);

        Model.Note FindByNote(long noteId, Model.Log log = null);

        List<Model.Note> FindByNotes(long id, string year, string month, Model.Log log = null);

        List<Model.Note> FindByUf(long companyId, string year, string month, string uf, Model.Log log = null);

        List<Model.Note> FindByCompany(long companyId, Model.Log log = null);
    }
}
