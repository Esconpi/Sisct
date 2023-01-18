using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteInventoryExitService : IServiceBase<Model.ProductNoteInventoryExit>
    {
        Task CreateRange(List<Model.ProductNoteInventoryExit> products, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByCompany(long companyId, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(long companyId, string year, string month, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNote(string chave, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByPeriod(long companyId, DateTime inicio, DateTime fim, Model.Log log = null);

        List<Model.ProductNoteInventoryExit> FindByNotes(long companyId, string year, Model.Log log = null);
    }
}
