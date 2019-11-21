
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INoteRepository : IRepository<Model.Note>
    {
        Model.Note FindByCompany(string company, Model.Log log = null);

        Model.Note FindByNote(string chave, Model.Log log = null);

        List<Model.Note> FindByNotes(int id, string year, string month, Model.Log log = null);

        List<Model.Note> FindByUf(int companyId, string year, string month,string uf, Model.Log log = null);

    }
}
