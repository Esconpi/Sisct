using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteInventoryExitRepository : IRepository<Model.ProductNoteInventoryExit>
    {

        void Create(List<Model.ProductNoteInventoryExit> products, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByCompany(int companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(int id, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNote(string chave , Model.Log log = null);
    }
}
