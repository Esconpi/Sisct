
using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteService : IServiceBase<Model.ProductNote>
    {
      
        List<Model.ProductNote> FindByNotes(int noteId, Model.Log log = null);

        List<Model.ProductNote> FindByProducts(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.Note> notes, int taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByNcmUfAliq(List<Model.Note> notes, string ncm, decimal aliq, Model.Log log = null);

        List<Model.ProductNote> FindByCnpjCprod(List<Model.Note> notes, string cnpj, string cprod, string ncm, string cest, Model.Log log = null);

        decimal FindByTotal(List<string> notes, Model.Log log = null);

        decimal FindBySubscription(List<Model.Note> notes, int taxaid, Model.Log log = null);

        List<Model.ProductNote> FindByTaxation(int noteId, Model.Log log = null);

        Model.ProductNote FindByProduct(int noteId, string nItem, Model.Log log = null);
    }
}
