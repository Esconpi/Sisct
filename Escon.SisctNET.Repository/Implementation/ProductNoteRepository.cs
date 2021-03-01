using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class ProductNoteRepository : Repository<Model.ProductNote>, IProductNoteRepository
    {
        private readonly ContextDataBase _context;

        public ProductNoteRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<ProductNote> FindByNcmUfAliq(List<Note> notes, string ncm, decimal aliq, string cest, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                var rst = _context.ProductNotes
                    .Where(_ => _.Ncm.Equals(ncm) && _.Picms.Equals(aliq) && _.NoteId.Equals(note.Id) && _.Cest.Equals(cest) && _.Pautado.Equals(false))
                    .Include(c => c.Note.Company)
                    .ToList();
                products.AddRange(rst);                
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByCnpjCprod(List<Note> notes, string cnpj, string cprod, string ncm, string cest, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                if (note.Cnpj.Equals(cnpj))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.Cprod.Equals(cprod) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByNote(int noteId, Log log)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId))
                .Include(c => c.Note.Company)
                .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNote> FindByProducts(List<Note> notes, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                var rst = _context.ProductNotes
                    .Where(_ => _.NoteId.Equals(note.Id))
                    .Include(c => c.Note.Company)
                    .ToList();
                products.AddRange(rst);
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByProductsType(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                if (taxationType.Equals(Model.TypeTaxation.Nenhum))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                if (taxationType.Equals(Model.TypeTaxation.ST))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AP))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(1)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.CO))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(2)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.COR))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(4)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.IM))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(3)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.Isento))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(7)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AT))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(8)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.NT))
                {
                    var rst = _context.ProductNotes
                        .Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(9)))
                        .Include(c => c.Note.Company)
                        .ToList();
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public decimal FindByTotal(List<int> notes, Log log = null)
        {
            decimal total = 0;
            foreach (var item in notes)
            {
                var notas = _context.Notes.Where(_ => _.Id.Equals(item)).FirstOrDefault();
                total += notas.Vnf;
            }
            AddLog(log);
            return total;
        }

        public decimal FindBySubscription(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            decimal icmsTotalSt = 0;

            if (taxationType.Equals(Model.TypeTaxation.ST) || taxationType.Equals(Model.TypeTaxation.AT))
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6) || _.TaxationTypeId.Equals(8))).Select(_ => _.IcmsST).Sum());
                }
            }
            else if (taxationType.Equals(Model.TypeTaxation.AP) || taxationType.Equals(Model.TypeTaxation.CO) ||
                    taxationType.Equals(Model.TypeTaxation.COR) || taxationType.Equals(Model.TypeTaxation.IM))
            {
                    foreach (var note in notes)
                    {
                        icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(1) || _.TaxationTypeId.Equals(2) || _.TaxationTypeId.Equals(3) || _.TaxationTypeId.Equals(4))).Select(_ => _.IcmsST).Sum());
                    }
            }
            return icmsTotalSt;
        }

        public List<ProductNote> FindByTaxation(int noteId, Log log = null)
        {

            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId) && _.Status.Equals(false))
                .Include(c => c.Note.Company)
                .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public ProductNote FindByProduct(int noteId, string nItem, Log log = null)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId) && _.Nitem.Equals(nItem))
                .Include(c => c.Note.Company)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<ProductNote> FindByCfopNotesIn(int companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;
            
            foreach (var note in notes)
            {
                result = _context.ProductNotes
                    .Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest.Equals(1) &&
                                 Array.Exists(cfopAtivo, e => e.Equals(_.Cfop)) && _.TaxationTypeId.Equals(1))
                    .Include(c => c.Note.Company)
                    .ToList();
            }

            return result;
        }

        public List<ProductNote> FindByCfopNotesOut(int companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;

            foreach (var note in notes)
            {
                result = _context.ProductNotes
                    .Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest != 1 && 
                                Array.Exists(cfopAtivo, e => e.Equals(_.Cfop)))
                    .Include(c => c.Note.Company)
                    .ToList();
            }

            return result;
        }

        public bool FindByNcmAnnex(int Annex, string ncm, Log log = null)
        {
            var ncms = _context.NcmConvenios.Where(_ => _.AnnexId.Equals(Annex)).Select(_ => _.Ncm);
            bool NcmIncentivo = false;
            foreach (var n in ncms)
            {
                int contaChar = n.Length;
                string substring = "";
                if (contaChar < 8)
                {
                    substring = ncm.Substring(0, contaChar);
                }
                else
                {
                    substring = ncm;
                }
               
                if (n.Equals(substring) && !contaChar.Equals(0))
                {
                    NcmIncentivo = true;
                    break;
                }
            }
            return NcmIncentivo;

        }

        public List<ProductNote> FindByIncentive(List<Note> notes, Log log = null)
        {
            List <ProductNote> products = new List<ProductNote>();

            foreach(var note in notes)
            {
                var itens = _context.ProductNotes
                    .Where(_ => _.NoteId.Equals(note.Id) && _.Incentivo == true)
                    .Include(c => c.Note.Company)
                    .ToList();
                products.AddRange(itens);
            }
            return products;
        }

        public List<ProductNote> FindByNormal(List<Note> notes, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();

            foreach (var note in notes)
            {
                var itens = _context.ProductNotes
                    .Where(_ => _.NoteId.Equals(note.Id) && _.Incentivo == false && _.Status.Equals(true))
                    .Include(c => c.Note.Company)
                    .ToList();
                products.AddRange(itens);
            }
            return products;
        }

        public void Create(List<ProductNote> products, Log log = null)
        {
            foreach (var p in products)
            {
                _context.ProductNotes.Add(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public void Delete(List<ProductNote> products, Log log = null)
        {
            foreach (var p in products)
            {
                _context.ProductNotes.Remove(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public void Update(List<ProductNote> products, Log log = null)
        {
            foreach (var p in products)
            {
                _context.ProductNotes.Update(p);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public ProductNote FindByProduct(int id, Log log = null)
        {
            var rst = _context.ProductNotes
              .Where(_ => _.Id.Equals(id))
              .Include(c => c.Note.Company)
              .FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
