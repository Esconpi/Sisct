using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface INoteInventoryExitRepository : IRepository<Model.NoteInventoryExit>
    {
        Model.NoteInventoryExit FindByNote(string chave, Model.Log log = null);

        Model.NoteInventoryExit FindByNote(int id, Model.Log log = null);

        List<Model.NoteInventoryExit> FindByCompany(int companyId, Model.Log log = null);

        List<Model.NoteInventoryExit> FindByNotes(int id, string year, string month, Model.Log log = null);
    }
}
