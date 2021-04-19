using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteInventoryExitRepository : IRepository<Model.ProductNoteInventoryExit>
    {

        void Create(List<Model.ProductNoteInventoryExit> products, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNote(int noteId, Model.Log log = null);
    }
}
