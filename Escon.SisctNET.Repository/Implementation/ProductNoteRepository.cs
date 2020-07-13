using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
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
                var rst = _context.ProductNotes.Where(_ => _.Ncm.Equals(ncm) && _.Picms.Equals(aliq) && _.NoteId.Equals(note.Id) && _.Cest.Equals(cest) && _.Pautado.Equals(false));
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
                    var rst = _context.ProductNotes.Where(_ => _.Cprod.Equals(cprod) && _.Ncm.Equals(ncm) && _.Cest.Equals(cest));
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByNotes(int noteId, Log log)
        {
            var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(noteId));
            AddLog(log);
            return rst.ToList();
        }

        public List<ProductNote> FindByProducts(List<Note> notes, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id));
                products.AddRange(rst);
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByProductsType(List<Note> notes, int taxationType, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                if (taxationType == 0)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id));
                    products.AddRange(rst);
                }
                if (taxationType == 1)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)));
                    products.AddRange(rst);
                }
                else if (taxationType == 2)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(1)));
                    products.AddRange(rst);
                }
                else if (taxationType == 3)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(2)));
                    products.AddRange(rst);
                }
                else if (taxationType == 4)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(4)));
                    products.AddRange(rst);
                }
                else if (taxationType == 5)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(3)));
                    products.AddRange(rst);
                }
                else if (taxationType == 6)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(7)));
                    products.AddRange(rst);
                }
                else if (taxationType == 7)
                {
                    var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationTypeId.Equals(8)));
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
        }

        public decimal FindByTotal(List<string> notes, Log log = null)
        {
            decimal total = 0;
            foreach (var item in notes)
            {
                var notas = _context.Notes.Where(_ => _.Nnf.Equals(item)).FirstOrDefault();
                total += notas.Vnf;
            }
            AddLog(log);
            return total;
        }

        public decimal FindBySubscription(List<Note> notes, int taxaid, Log log = null)
        {
            decimal icmsTotalSt = 0;

            if (taxaid == 1)
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.Nnf.Equals(note.Nnf) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6) || _.TaxationTypeId.Equals(8))).Select(_ => _.IcmsST).Sum());
                }
            }
            else if (taxaid == 2)
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.Nnf.Equals(note.Nnf) && (_.TaxationTypeId.Equals(1) || _.TaxationTypeId.Equals(2) || _.TaxationTypeId.Equals(3) || _.TaxationTypeId.Equals(4))).Select(_ => _.IcmsST).Sum());
                }
            }
            return icmsTotalSt;
        }

        public List<ProductNote> FindByTaxation(int noteId, Log log = null)
        {

            var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(noteId) && _.Status.Equals(false));
            AddLog(log);
            return rst.ToList();
        }

        public ProductNote FindByProduct(int noteId, string nItem, Log log = null)
        {
            var rst = _context.ProductNotes.Where(_ => _.NoteId.Equals(noteId) && _.Nitem.Equals(nItem)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<ProductNote> FindByCfopNotesIn(int companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;
            
            foreach (var note in notes)
            {
                result = _context.ProductNotes.Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest.Equals(1) &&
                                                           Array.Exists(cfopAtivo, e => e.Equals(_.Cfop)) && _.TaxationTypeId.Equals(1)).ToList();
            }

            return result;
        }

        public List<ProductNote> FindByCfopNotesOut(int companyId, List<Note> notes, Log log = null)
        {
            var cfopAtivo = _context.CompanyCfops.Where(_ => _.CompanyId.Equals(companyId) && _.Active.Equals(true)).Select(_ => _.Cfop.Code).ToArray();

            List<ProductNote> result = null;

            foreach (var note in notes)
            {
                result = _context.ProductNotes.Where(_ => _.Note.CompanyId.Equals(companyId) && _.Note.IdDest != 1 && 
                                                          Array.Exists(cfopAtivo, e => e.Equals(_.Cfop))).ToList();
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
                var itens = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && _.Incentivo == true).ToList();
                products.AddRange(itens);
            }
            return products;
        }

        public List<ProductNote> FindByNormal(List<Note> notes, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();

            foreach (var note in notes)
            {
                var itens = _context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && _.Incentivo == false && _.Status.Equals(true)).ToList();
                products.AddRange(itens);
            }
            return products;
        }

        public List<Product> FindAllInDate(DateTime dateProd, Log log = null)
        {
            List<Product> products = new List<Product>();

            var productPauta = _context.Products;

            foreach(var prod in productPauta)
            {
                var dataInicial = DateTime.Compare(prod.DateStart, dateProd);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), dateProd);

                if (dataInicial <= 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }else if (dataInicial <= 0 && dataFinal > 0)
                {
                    products.Add(prod);
                    continue;
                }
            }

            return products;
        }

        public List<Product1> FindAllInDate1(DateTime dateProd, Log log = null)
        {
            List<Product1> products = new List<Product1>();

            var productPauta = _context.Product1s;

            foreach (var prod in productPauta)
            {
                var dataInicial = DateTime.Compare(prod.DateStart, dateProd);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(prod.DateEnd), dateProd);

                if (dataInicial <= 0 && prod.DateEnd == null)
                {
                    products.Add(prod);
                    continue;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    products.Add(prod);
                    continue;
                }
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
    }
}
