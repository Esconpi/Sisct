using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("productnote")]
    public class ProductNote : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Código do Produto")]
        public string Cprod { get; set; }

        [Display(Name = "NCM")]
        public string Ncm { get; set; }

        [Display(Name = "Incentivo")]
        public bool Incentivo { get; set; }

        [Display(Name = "CEST")]
        public string Cest { get; set; }

        [Display(Name = "CFOP")]
        public string Cfop { get; set; }

        [Display(Name = "Produto")]
        public string Xprod { get; set; }

        [Display(Name = "Valor")]
        public decimal Vprod { get; set; }

        [Display(Name = "Quantidade")]
        public decimal ? Qcom { get; set; }

        [Display(Name = "Quantidade da Pauta")]
        public decimal ? Qpauta { get; set; }

        [Display(Name = "Unidade")]
        public string Ucom { get; set; }

        [Display(Name = "Valor Unitário")]
        public decimal Vuncom { get; set; }

        [Display(Name = "Valor ICMS")]
        public decimal Vicms { get; set; }

        [Display(Name = "Aliquota Interestadual")]
        public decimal Picms { get; set; }

        [Display(Name = "Aliquota Interestadual Origem")]
        public decimal PicmsOrig { get; set; }

        [Display(Name = "Valor IPI")]
        public decimal Vipi { get; set; }

        [Display(Name = "Valor PIS")]
        public decimal Vpis { get; set; }

        [Display(Name = "Valor COFINS")]
        public decimal Vcofins { get; set; }

        [Display(Name = "Base de Cálculo")]
        public decimal Vbasecalc { get; set; }

        [Display(Name = "Valor Frete")]
        public decimal Vfrete { get; set; }

        [Display(Name = "Seguro")]
        public decimal Vseg { get; set; }

        [Display(Name = "Desconto")]
        public decimal Vdesc { get; set; }

        [Display(Name = "Outras Despesas")]
        public decimal Voutro { get; set; }

        [Display(Name = "BCR(%)")]
        public decimal? BCR { get; set; }

        [Display(Name = "Aliq. Fecop")]
        public decimal? Fecop { get; set; }

        [Display(Name = "Aliquota Interna")]
        public decimal? AliqInterna { get; set; }

        [Display(Name = "MVA")]
        public decimal? Mva { get; set; }

        public bool Status { get; set; }

        [Display(Name = "Base de Cálculo ICMS")]
        public decimal? Valoragregado { get; set; }

        [Display(Name = "Base de Cálculo Reduzida")]
        public decimal? ValorBCR { get; set; }

        [Display(Name = "CTe")]
        public decimal IcmsCTe { get; set; }

        [Display(Name = "ICMS ST")]
        public decimal? IcmsST { get; set; }

        [Display(Name = "ICMS ST RET")]
        public decimal? VicmsStRet { get; set; }

        [Display(Name = "Base Calculo Fecop ST")]
        public decimal? VbcFcpSt { get; set; }

        [Display(Name = "Base Calculo Fecop ST RET")]
        public decimal? VbcFcpStRet { get; set; }

        [Display(Name = "Aliq Fecop ST")]
        public decimal? pFCPST { get; set; }

        [Display(Name = "Aliq Fecop ST RET")]
        public decimal? pFCPSTRET { get; set; }

        [Display(Name = "Fecop ST")]
        public decimal? VfcpST { get; set; }

        [Display(Name = "Fecop ST RET")]
        public decimal? VfcpSTRet { get; set; }

        [Display(Name = "Valor do Imp. Antes Credito")]
        public decimal? ValorAC { get; set; }

        [Display(Name = "Produto Pautado")]
        public bool Pautado { get; set; }

        [Display(Name = "Dif")]
        public decimal? Diferencial { get; set; }

        [Display(Name = "ICMS Apurado")]
        public decimal? IcmsApurado { get; set; }

        [Display(Name = "ICMS Apurado CTe")]
        public decimal? IcmsApuradoCTe { get; set; }

        [Display(Name = "Total ICMS")]
        public decimal? TotalICMS { get; set; }

        [Display(Name = "Total FECOP")]
        public decimal? TotalFecop { get; set; }

        [Display(Name = "Valor Frete")]
        public decimal Freterateado { get; set; }


        [Display(Name = "Tipo de Uso")]
        [ForeignKey("TaxationType")]
        public long? TaxationTypeId { get; set; }

        private TaxationType taxationType;
        public TaxationType TaxationType
        {
            get => LazyLoader.Load(this, ref taxationType);
            set => taxationType = value;
        }

        [Display(Name = "Produto")]
        [ForeignKey("Product")]
        public long? ProductId { get; set; }

        private Product product;
        public Product Product
        {
            get => LazyLoader.Load(this, ref product);
            set => product = value;
        }

        [Display(Name = "Produto1")]
        [ForeignKey("Product1")]
        public long? Product1Id { get; set; }

        private Product1 product1;
        public Product1 Product1
        {
            get => LazyLoader.Load(this, ref product1);
            set => product1 = value;
        }

        [Display(Name = "Produto2")]
        [ForeignKey("Product2")]
        public long? Product2Id { get; set; }

        private Product2 product2;
        public Product2 Product2
        {
            get => LazyLoader.Load(this, ref product2);
            set => product2 = value;
        }


        [Display(Name = "Produto3")]
        [ForeignKey("Product3")]
        public long? Product3Id { get; set; }

        private Product3 product3;
        public Product3 Product3
        {
            get => LazyLoader.Load(this, ref product3);
            set => product3 = value;
        }

        [Display(Name = "Nota")]
        [ForeignKey("Note")]
        public long? NoteId { get; set; }

        private Note note;
        public Note Note
        {
            get => LazyLoader.Load(this, ref note);
            set => note = value;
        }

        [Display(Name = "Número do Item")]
        public string Nitem { get; set; }

        [Display(Name = "Origem")]
        public int ? Orig { get; set; }

        [Display(Name = "Validade Inicial")]
        public DateTime ? DateStart { get; set; }

        public string Produto { get; set; }

        public decimal? PercentualInciso { get; set; }

        [Display(Name = "BCR")]
        public bool EBcr { get; set; }

        public decimal? AliqInternaCTe { get; set; }
    }
}
