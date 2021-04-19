using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface INoteInventoryEntryService : IServiceBase<Model.NoteInventoryEntry>
    {
        Model.NoteInventoryEntry FindByNote(string chave, Model.Log log = null);

        Model.NoteInventoryEntry FindByNote(int id, Model.Log log = null);

        List<Model.NoteInventoryEntry> FindByCompany(int companyId, Model.Log log = null);

        List<Model.NoteInventoryEntry> FindByNotes(int id, string year, string month, Model.Log log = null);
    }
}
