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

        public decimal FindBySubscription(List<Note> notes, Model.TypeTaxation taxationType, Log log = null)
        {
            decimal icmsTotalSt = 0;

            if (taxationType.Equals(Model.TypeTaxation.ST) || taxationType.Equals(Model.TypeTaxation.STMVA) ||
                taxationType.Equals(Model.TypeTaxation.STMPAUTA))
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationType.Description.Equals("2  ST - Subs.Tributária") ||
                                                                                      _.TaxationType.Description.Equals("2  Base de Cálculo Reduzida")))
                                                                          .Select(_ => _.IcmsST)
                                                                          .Sum());
                }
            }
            if (taxationType.Equals(Model.TypeTaxation.AT))
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationType.Description.Equals("2 AT - Antecipacao Total")))
                                                                          .Select(_ => _.IcmsST)
                                                                          .Sum());
                }
            }
            else if (taxationType.Equals(Model.TypeTaxation.AP) || taxationType.Equals(Model.TypeTaxation.CO) ||
                    taxationType.Equals(Model.TypeTaxation.COR) || taxationType.Equals(Model.TypeTaxation.IM))
            {
                foreach (var note in notes)
                {
                    icmsTotalSt += Convert.ToDecimal(_context.ProductNotes.Where(_ => _.NoteId.Equals(note.Id) && (_.TaxationType.Description.Equals("1  AP - Antecipação parcial") ||
                                                                                      _.TaxationType.Description.Equals("1  CO - Consumo-Dif. Aliquota") ||
                                                                                      _.TaxationType.Description.Equals("1  CR - Consumo/Revenda-Dif.Aliquota") ||
                                                                                      _.TaxationType.Description.Equals("1  IM - Imobilizado-Dif. Aliquota")))
                                                                           .Select(_ => _.IcmsST)
                                                                           .Sum());
                }
            }
            return icmsTotalSt;
        }

        public ProductNote FindByProduct(long id, Log log = null)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.Id.Equals(id))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
                .FirstOrDefault();
            AddLog(log);
            return rst;
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

        public List<ProductNote> FindByNote(long noteId, Log log)
        {
            var rst = _context.ProductNotes
                .Where(_ => _.NoteId.Equals(noteId))
                .Include(n => n.Note)
                .Include(c => c.Note.Company)
                .Include(t => t.TaxationType)
                .Include(p => p.Product)
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
                else if (taxationType.Equals(Model.TypeTaxation.ST) || taxationType.Equals(Model.TypeTaxation.STMVA) ||
                        taxationType.Equals(Model.TypeTaxation.STMPAUTA))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("2  ST - Subs.Tributária") || _.TaxationType.Description.Equals("2  Base de Cálculo Reduzida"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AP))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("1  AP - Antecipação parcial"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.CO))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("1  CO - Consumo-Dif. Aliquota"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.COR))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("1  CR - Consumo/Revenda-Dif.Aliquota"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.IM))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("1  IM - Imobilizado-Dif. Aliquota"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.Isento))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("Isento"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.AT))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("2 AT - Antecipacao Total"))
                        .ToList();
                    products.AddRange(rst);
                }
                else if (taxationType.Equals(Model.TypeTaxation.NT))
                {
                    var rst = note.Products
                        .Where(_ => _.TaxationType.Description.Equals("Não Tributado"))
                        .ToList();
                    products.AddRange(rst);
                }
            }
            AddLog(log);
            return products;
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
            else if (taxationType.Equals(Model.TypeTaxation.ST) || taxationType.Equals(Model.TypeTaxation.STMVA) ||
                     taxationType.Equals(Model.TypeTaxation.STMPAUTA))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("2  ST - Subs.Tributária") || _.TaxationType.Description.Equals("2  Base de Cálculo Reduzida"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.AP))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("1  AP - Antecipação parcial"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.CO))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("1  CO - Consumo-Dif. Aliquota"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.COR))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("1  CR - Consumo/Revenda-Dif.Aliquota"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.IM))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("1  IM - Imobilizado-Dif. Aliquota"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.Isento))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("Isento"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.AT))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("2 AT - Antecipacao Total"))
                    .ToList();
                products.AddRange(rst);
            }
            else if (taxationType.Equals(Model.TypeTaxation.NT))
            {
                var rst = productNotes
                    .Where(_ => _.TaxationType.Description.Equals("Não Tributado"))
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
    }
}
