using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteInventoryEntryRepository : IRepository<Model.ProductNoteInventoryEntry>
    {
        void Create(List<Model.ProductNoteInventoryEntry> products, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNote(int noteId, Model.Log log = null);
    }
}
