using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface IProductNoteService : IServiceBase<Model.ProductNote>
    {
        void Create(List<Model.ProductNote> products, Model.Log log = null);

        void Delete(List<Model.ProductNote> products, Model.Log log = null);

        void Update(List<Model.ProductNote> products, Model.Log log = null);

        decimal FindByTotal(List<long> notes, Model.Log log = null);

        decimal FindBySubscription(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        Model.ProductNote FindByProduct(long noteId, string item, Model.Log log = null);

        Model.ProductNote FindByProduct(long id, Model.Log log = null);

        List<Model.ProductNote> FindByNote(long noteId, Model.Log log = null);

        List<Model.ProductNote> FindByProducts(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.ProductNote> productNotes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByNcmUfAliq(List<Model.Note> notes, string ncm, decimal aliq, string cest, Model.Log log = null);

        List<Model.ProductNote> FindByCnpjCprod(List<Model.Note> notes, string cnpj, string cprod, string ncm, string cest, Model.Log log = null);

        List<Model.ProductNote> FindByTaxation(long noteId, Model.Log log = null);

        List<Model.ProductNote> FindByTaxation(List<Model.ProductNote> productNotes, Model.Log log = null);

        List<Model.ProductNote> FindByCfopNotesIn(long companyId, List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByCfopNotesOut(long companyId, List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByIncentive(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByNormal(List<Model.Note> notes, Model.Log log = null);
    }
}
