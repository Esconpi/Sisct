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

        public List<ProductNote> FindByNcmUfAliq(List<Note> notes, string ncm, decimal aliq, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                var rst = _context.ProductNotes.Where(_ => _.Ncm.Equals(ncm) && _.Picms.Equals(aliq) && _.NoteId.Equals(note.Id));
                foreach (var item in rst)
                {
                    products.Add(item);
                }
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
                    foreach (var item in rst)
                    {
                        products.Add(item);
                    }
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
                foreach (var item in rst)
                {
                    products.Add(item);
                }
            }
            AddLog(log);
            return products;
        }

        public List<ProductNote> FindByProductsType(List<Note> notes, int taxationType, Log log = null)
        {
            List<ProductNote> products = new List<ProductNote>();
            foreach (var note in notes)
            {
                if (taxationType == 1)
                {
                    var rst = _context.ProductNotes.Where(_ => _.Nnf.Equals(note.Nnf) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6)));
                    foreach (var item in rst)
                    {
                        products.Add(item);
                    }
                }
                else if (taxationType == 2)
                {
                    var rst = _context.ProductNotes.Where(_ => _.Nnf.Equals(note.Nnf) && (_.TaxationTypeId.Equals(1) || _.TaxationTypeId.Equals(2) || _.TaxationTypeId.Equals(3) || _.TaxationTypeId.Equals(4)));
                    foreach (var item in rst)
                    {
                        products.Add(item);
                    }
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
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.Nnf.Equals(note.Nnf) && (_.TaxationTypeId.Equals(5) || _.TaxationTypeId.Equals(6))).Select(_ => _.IcmsST).Sum());
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
                string substring = ncm.Substring(0, contaChar);
                if (n.Equals(substring))
                {
                    NcmIncentivo = true;
                    break;
                }
            }
            return NcmIncentivo;

        }


    }
}
