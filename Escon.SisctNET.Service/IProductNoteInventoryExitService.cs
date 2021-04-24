using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteInventoryExitService : IServiceBase<Model.ProductNoteInventoryExit>
    {
        void Create(List<Model.ProductNoteInventoryExit> products, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByCompany(int companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(int companyId, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNote(string chave, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByPeriod(int companyId, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(int companyId, string year, Model.Log log = null);
    }
}
