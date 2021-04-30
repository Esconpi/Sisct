using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteInventoryExitRepository : IRepository<Model.ProductNoteInventoryExit>
    {

        void Create(List<Model.ProductNoteInventoryExit> products, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByCompany(long companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(long companyId, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNote(string chave , Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByPeriod(long companyId, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(long companyId, string year, Model.Log log = null);
    }
}
