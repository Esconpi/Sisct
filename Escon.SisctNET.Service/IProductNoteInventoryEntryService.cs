using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteInventoryEntryService : IServiceBase<Model.ProductNoteInventoryEntry>
    {

        void Create(List<Model.ProductNoteInventoryEntry> products, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByCompany(int companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNotes(int companyId, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNote(string chave, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByPeriod(int companyId, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.ProductNoteInventoryEntry> FindByNotes(int companyId, string year, Model.Log log = null);
    }
}
