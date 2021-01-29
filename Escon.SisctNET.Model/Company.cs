using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("company")]
    public class Company : EntityBase
    {
        [JsonIgnore]
        public ILazyLoader LazyLoader { get; set; }

        [Display(Name = "Ativa")]
        public bool Active { get; set; }

        [Display(Name = "UF")]
        public bool Status { get; set; }

        [Display(Name = "Incentivada")]
        public bool Incentive { get; set; }

        [Display(Name = "Apuração do regime de ICMS")]
        public bool TipoApuracao { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Razão Social")]
        public string SocialName { get; set; }

        [Display(Name = "Nome Fantasia")]
        public string FantasyName { get; set; }

        [Display(Name = "Cnpj/CPF")]
        public string Document { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "Capítulo")]
        [ForeignKey("Chapter")]
        public int? ChapterId { get; set; }

        private Chapter chapter;
        public Chapter Chapter
        {
            get => LazyLoader.Load(this, ref chapter);
            set => chapter = value;
        }

        [Display(Name = "Anexo")]
        [ForeignKey("Annex")]
        public int? AnnexId { get; set; }

        private Annex annex;
        public Annex Annex
        {
            get => LazyLoader.Load(this, ref annex);
            set => annex = value;
        }

        [Display(Name = "Tipo")]
        [ForeignKey("CountingType")]
        public int? CountingTypeId { get; set; }

        private CountingType countingType;
        public CountingType CountingType
        {
            get => LazyLoader.Load(this, ref countingType);
            set => countingType = value;
        }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Cidade")]
        [ForeignKey("County")]
        public int? CountyId { get; set; }

        private County county;
        public County County
        {
            get => LazyLoader.Load(this, ref county);
            set => county = value;
        }

        [Display(Name = "Inscrição Estadual")]
        public string Ie { get; set; }

        [Display(Name = "Logradouro")]
        public string Logradouro { get; set; }

        [Display(Name = "Número")]
        public string Number { get; set; }

        [Display(Name = "Complemento")]
        public string Complement { get; set; }

        [Display(Name = "Bairro")]
        public string District { get; set; }

        [Display(Name = "Cep")]
        public string Cep { get; set; }

        [Display(Name = "Telefone")]
        public string Phone { get; set; }

        [Display(Name = "ICMS %")]
        public decimal? Icms { get; set; }

        [Display(Name = "Funef %")]
        public decimal? Funef { get; set; }

        [Display(Name = "Cotac %")]
        public decimal? Cotac { get; set; }

        [Display(Name = "Suspensão %")]
        public decimal? Suspension { get; set; }

        public decimal? VendaCpf { get; set; }

        public decimal? VendaCpfExcedente { get; set; }

        public decimal? VendaContribuinte { get; set; }

        public decimal? VendaContribuinteExcedente { get; set; }

        public decimal? Transferencia { get; set; }

        public decimal? TransferenciaExcedente { get; set; }

        [Display(Name = "Venda p/ Mesmo Grupo %")]
        public decimal? VendaMGrupo { get; set; }

        public decimal? VendaMGrupoExcedente { get; set; }

        [Display(Name = "Transferência Interestaduais %")]
        public decimal? TransferenciaInter { get; set; }

        public decimal? TransferenciaInterExcedente { get; set; }

        [Display(Name = "Venda do Anexo %")]
        public decimal? VendaAnexo { get; set; }

        public decimal? VendaAnexoExcedente { get; set; }

        [Display(Name = "Fecop %")]
        public decimal? Fecop { get; set; }

        public decimal? Faturamento { get; set; }

        public decimal? FaturamentoExcedente { get; set; }

        [Display(Name = "Icms p/ Não Contribuinte %")]
        public decimal? IcmsNContribuinte { get; set; }

        [Display(Name = "Icms p/ Não Contribuinte Fora do Estado %")]
        public decimal? IcmsNContribuinteFora { get; set; }

        [Display(Name = "Tipo")]
        public bool TypeCompany { get; set; }

        [Display(Name = "Icms Aliq. SUperior a 25 %")]
        public decimal? IcmsAliqM25 { get; set; }

        [Display(Name = "Seção")]
        [ForeignKey("Section")]
        public int? SectionId { get; set; }

        private Section section;
        public Section Section
        {
            get => LazyLoader.Load(this, ref section);
            set => section = value;
        }

        [Display(Name = "Aliq. Interna %")]
        public decimal ? AliqInterna { get; set; }

        [Display(Name = "Inc. I Interna %")]
        public decimal ? IncIInterna { get; set; }

        [Display(Name = "Inc. I Interestadual %")]
        public decimal? IncIInterestadual { get; set; }

        [Display(Name = "Inc. II Interna %")]
        public decimal? IncIIInterna { get; set; }

        [Display(Name = "Inc. II Interestadual %")]
        public decimal? IncIIInterestadual { get; set; }

        public decimal? VendaArt781 { get; set; }

        public decimal? VendaArt781Excedente { get; set; }

        [Display(Name = "IRPJ Combustível %")]
        public decimal? IRPJ1 { get; set; }

        [Display(Name = "IRPJ Comércio %")]
        public decimal? IRPJ2 { get; set; }

        [Display(Name = "IRPJ Transporte %")]
        public decimal? IRPJ3 { get; set; }

        [Display(Name = "IRPJ Serviço %")]
        public decimal? IRPJ4 { get; set; }

        [Display(Name = "CSLL Comércio %")]
        public decimal? CSLL1 { get; set; }

        [Display(Name = "CSLL Serviço %")]
        public decimal? CSLL2 { get; set; }

        [Display(Name = "CPRB %")]
        public decimal? CPRB { get; set; }

        public bool StatusCPRB { get; set; }

        [Display(Name = "PIS %")]
        public decimal? PercentualPis { get; set; }

        [Display(Name = "COFINS %")]
        public decimal? PercentualCofins { get; set; }

        [Display(Name = "IRPJ Normal %")]
        public decimal? PercentualIRPJ { get; set; }

        [Display(Name = "CSLL Normal %")]
        public decimal? PercentualCSLL { get; set; }

        [Display(Name = "Adicional IRPJ %")]
        public decimal? AdicionalIRPJ { get; set; }

        public bool Sped { get; set; }

        public decimal? PercentualPisRF { get; set; }

        public decimal? PercentualPisCredito { get; set; }

        public decimal? PercentualCofinsRF { get; set; }        

        public decimal? PercentualCofinsCredito { get; set; }

        public virtual IList<EmailResponsible> EmaiResponsibles { get; set; }
    }
}