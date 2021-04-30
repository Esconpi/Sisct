using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteInventoryEntryRepository : IRepository<Model.ProductNoteInventoryEntry>
    {

        void Create(List<Model.ProductNoteInventoryEntry> products, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByCompany(long companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNotes(long companyId, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNote(string chave, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByPeriod(long companyId, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNotes(long companyId, string year, Model.Log log = null);
    }
}
