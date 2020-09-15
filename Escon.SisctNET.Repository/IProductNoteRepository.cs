using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IProductNoteRepository : IRepository<Model.ProductNote>
    {
        void Create(List<Model.ProductNote> products, Model.Log log = null);

        void Delete(List<Model.ProductNote> products, Model.Log log = null);

        void Update(List<Model.ProductNote> products, Model.Log log = null);

        List<Model.ProductNote> FindByNotes(int noteId, Model.Log log = null);

        List<Model.ProductNote> FindByProducts(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByProductsType(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByNcmUfAliq(List<Model.Note> notes, string ncm, decimal aliq, string cest, Model.Log log = null);

        List<Model.ProductNote> FindByCnpjCprod(List<Model.Note> notes, string cnpj, string cprod, string ncm, string cest, Model.Log log = null);

        decimal FindByTotal(List<string> notes, Model.Log log = null);

        decimal FindBySubscription(List<Model.Note> notes, Model.TypeTaxation taxationType, Model.Log log = null);

        List<Model.ProductNote> FindByTaxation(int noteId, Model.Log log = null);

        Model.ProductNote FindByProduct(int noteId, string item, Model.Log log = null);

        List<Model.ProductNote> FindByCfopNotesIn(int companyId, List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByCfopNotesOut(int companyId, List<Model.Note> notes, Model.Log log = null);

        bool FindByNcmAnnex(int Annex, string ncm, Model.Log log = null);

        List<Model.ProductNote> FindByIncentive(List<Model.Note> notes, Model.Log log = null);

        List<Model.ProductNote> FindByNormal(List<Model.Note> notes, Model.Log log = null);

        List<Model.Product> FindAllInDate(DateTime dateProd, Model.Log log = null);

        List<Model.Product1> FindAllInDate1(DateTime dateProd, Model.Log log = null);

        List<Model.Product2> FindAllInDate2(DateTime dateProd, Model.Log log = null);

    }
}
