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
                var rst = note.Products
                    .Where(_ => _.Ncm.Equals(ncm) && _.Picms.Equals(aliq) && _.Cest.Equals(cest) && _.Pautado.Equals(false))
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
                    var rst = note.Products
                        .Where(_ => _.Cprod.Equals(cprod) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest))
                        .ToList();
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByNote(long noteId, Log log)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
                .Include(p1 => p1.Product1)
                .Include(p2 => p2.Product2)
                .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNote> FindByProducts(List<Note> notes, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                var rst = note.Products
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
                    var rst = note.Products
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.ST))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)5) || _.TaxationTypeId.Equals((long)6))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AP))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)1))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.CO))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)2))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.COR))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)4))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.IM))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)3))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.Isento))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)7))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AT))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)8))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.NT))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationTypeId.Equals((long)9))
                        .ToList();
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public decimal FindByTotal(List<long> notes, Log log = null)
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
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals((long)5) || _.TaxationTypeId.Equals((long)6) || _.TaxationTypeId.Equals(8))).Select(_ => _.IcmsST).Sum());
                }
            }
            else if (taxationType.Equals(Model.TypeTaxation.AP) || taxationType.Equals(Model.TypeTaxation.CO) ||
                    taxationType.Equals(Model.TypeTaxation.COR) || taxationType.Equals(Model.TypeTaxation.IM))
            {
                    foreach (var note in notes)
                    {
                        icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals((long)1) || _.TaxationTypeId.Equals((long)2) || _.TaxationTypeId.Equals((long)3) || _.TaxationTypeId.Equals((long)4))).Select(_ => _.IcmsST).Sum());
                    }
            }
            return icmsTotalSt;
        }

        public List<ProductNote> FindByTaxation(long noteId, Log log = null)
        {

            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId) && _.Status.Equals(false))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
                .Include(p1 => p1.Product1)
                .Include(p2 => p2.Product2)
                .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public ProductNote FindByProduct(long noteId, string nItem, Log log = null)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId) && _.Nitem.Equals(nItem))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
                .Include(p1 => p1.Product1)
                .Include(p2 => p2.Product2)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<ProductNote> FindByCfopNotesIn(long companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;
            
            foreach (var note in notes)
            {
                result = _context.ProductNotes
                    .Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest.Equals(1) &&
                                 Array.Exists(cfopAtivo, e => e.Equals(_.Cfop)) && _.TaxationTypeId.Equals(1))
                    .Include(n => n.Note)
                    .Include(c => c.Note.Company)
                    .Include(t => t.TaxationType)
                    .Include(p => p.Product)
                    .Include(p1 => p1.Product1)
                    .Include(p2 => p2.Product2)
                    .ToList();
            }

            return result;
        }

        public List<ProductNote> FindByCfopNotesOut(long companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;

            foreach (var note in notes)
            {
                result = note.Products
                    .Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest != 1 && 
                                Array.Exists(cfopAtivo, e => e.Equals(_.Cfop)))
                    .ToList();
            }

            return result;
        }

        public List<ProductNote> FindByIncentive(List<Note> notes, Log log = null)
        {
            List <ProductNote> products = new List<ProductNote>();

            foreach(var note in notes)
            {
                var itens = note.Products
                    .Where(_ => _.Incentivo == true)
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
                var itens = note.Products
                    .Where(_ => _.Incentivo == false && _.Status.Equals(true))
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

        public ProductNote FindByProduct(long id, Log log = null)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.Id.Equals(id))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
                .Include(p1 => p1.Product1)
                .Include(p2 => p2.Product2)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<ProductNote> FindByProductsType(List<ProductNote> productNotes, TypeTaxation taxationType, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();

            if (taxationType.Equals(Model.TypeTaxation.Nenhum))
            {
                var rst = productNotes
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.ST))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)5) || _.TaxationTypeId.Equals((long)6))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.AP))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)1))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.CO))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)2))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.COR))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)4))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.IM))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)3))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.Isento))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)7))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.AT))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)8))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.NT))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationTypeId.Equals((long)9))
                    .ToList();
                products.AddRange(rst);
            }

            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByTaxation(List<ProductNote> productNotes, Log log = null)
        {
            var rst = productNotes
               .Where(_ => _.Status.Equals(false))
               .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNote> FindByCompany(long companyId, Log log = null)
        {
            var rst = _context.ProductNotes
             .Where(_ => _.Note.CompanyId.Equals(companyId))
             .ToList();
            AddLog(log);
            return rst.ToList();
        }
    }
}
