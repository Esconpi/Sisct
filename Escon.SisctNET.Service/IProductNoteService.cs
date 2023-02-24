using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteService : IServiceBase<Model.ProductNote>
    {
        decimal FindBySubscription(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        Model.ProductNote FindByProduct(long id, Model.Log log = null);

        List<Model.ProductNote> FindByNote(long noteId, Model.Log log = null);

        List<Model.ProductNote> FindByProducts(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.ProductNote> productNotes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByNcmUfAliq(List<Model.Note> notes, string ncm, decimal aliq, string cest, Model.Log log = null);

        List<Model.ProductNote> FindByTaxation(List<Model.ProductNote> productNotes, Model.Log log = null);
    }
}
