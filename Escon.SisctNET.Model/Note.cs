using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("note")]
    public class Note : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Empresa")]
        [ForeignKey("Company")]
        public Nullable<int> CompanyId { get; set; }

        public virtual Company Company { get; set; }


        [Display(Name = "Chave")]
        public string Chave { get; set; }

        [Display(Name = "Nº Nota")]
        public string Nnf { get; set; }

        [Display(Name = "Mes")]
        public string MesRef { get; set; }

        [Display(Name = "Ano")]
        public string AnoRef { get; set; }

        [Display(Name = "CNPJ/CPF")]
        public string Cnpj { get; set; }

        [Display(Name = "CRT")]
        public string Crt { get; set; }

        [Display(Name = "UF")]
        public string Uf { get; set; }

        [Display(Name = "Insc. Estadual")]
        public string Ie { get; set; }

        [Display(Name = "Nº Frete")]
        public string Nct { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Emissão")]
        public DateTime Dhemi { get; set; }

        [Display(Name = "Total Nota")]
        public decimal Vnf { get; set; }

        [Display(Name = "Fornecedor")]
        public string Xnome { get; set; }

        [Display(Name = "IE Substituto Tributário")]
        public string Iest { get; set;}

        [Display(Name = "Gnre Paga")]
        public decimal? GnrePaga { get; set; }

        [Display(Name = "Gnre não Paga")]
        public decimal? GnreNPaga { get; set; }

        [Display(Name = "ICMS Pago AP")]
        public decimal? IcmsAp { get; set; }

        [Display(Name = "Icms Pago ST")]
        public decimal? IcmsSt { get; set; }

        [Display(Name = "Icms Pago CO")]
        public decimal? IcmsCo { get; set; }

        [Display(Name = "Icms Pago IM")]
        public decimal? IcmsIm { get; set; }

        [Display(Name = "Gnre paga AP")]
        public decimal? GnreAp { get; set; }

        [Display(Name = "Gnre paga CO")]
        public decimal? GnreCo { get; set; }

        [Display(Name = "Gnre paga IM")]
        public decimal? GnreIm { get; set; }

        [Display(Name = "Gnre paga ST")]
        public decimal? GnreSt { get; set; }

        [Display(Name = "Gnre não paga AP")]
        public decimal? GnreNAp { get; set; }

        [Display(Name = "Gnre não paga CO")]
        public decimal? GnreNCo { get; set; }

        [Display(Name = "Gnre não paga IM")]
        public decimal? GnreNIm { get; set; }

        [Display(Name = "Gnre não paga ST")]
        public decimal? GnreNSt { get; set; }      

        [Display(Name = "Fecop pago (1%)")]
        public decimal? Fecop1 { get; set; }

        [Display(Name = "Fecop pago (2%)")]
        public decimal? Fecop2 { get; set; }

        [Display(Name = "Fecop Gnre paga (1%)")]
        public decimal? FecopGnre1 { get; set; }

        [Display(Name = "Fecop Gnre paga (2%)")]
        public decimal? FecopGnre2 { get; set; }

        [Display(Name = "Gnre não Paga Fecop")]
        public decimal? GnreFecop { get; set; }

        [Display(Name = "Desconto")]
        public decimal? Desconto { get; set; }

        [Display(Name = "Valor Frete")]
        public decimal? Frete { get; set; }

        [Display(Name = "Tributada")]
        public bool Status { get; set; }

        public bool View { get; set; }

        public int ? IdDest { get; set; }
    }
}
