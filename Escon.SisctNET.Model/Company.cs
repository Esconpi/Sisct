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

        [Display(Name = "Razão Social")]
        public string SocialName { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Nome Fantasia")]
        public string FantasyName { get; set; }

        [Display(Name = "Cnpj/CPF")]
        public string Document { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Código")]
        public string Code { get; set; }

        [Display(Name = "ICMS %")]
        public decimal ? Icms { get; set; }

        [Display(Name = "Funef %")]
        public decimal ? Funef { get; set; }

        [Display(Name = "Cotac %")]
        public decimal ? Cotac { get; set; }

        [Display(Name = "Suspensão %")]
        public decimal ? Suspension { get; set; }

        public decimal ? VendaCpf { get; set; }

        public decimal? VendaCpfExcedente { get; set; }

        public decimal ? VendaContribuinte { get; set; }

        public decimal? VendaContribuinteExcedente { get; set; }

        public decimal ? Transferencia { get; set; }

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

        [Display(Name = "Fecop")]
        public decimal ? Fecop { get; set; }

        [Display(Name = "Anexo")]
        [ForeignKey("Annex")]
        public int ? AnnexId { get; set; }

        private Annex annex;
        public Annex Annex
        {
            get => LazyLoader.Load(this, ref annex);
            set => annex = value;
        }

        [Display(Name = "Tipo")]
        [ForeignKey("CountingType")]
        public int ? CountingTypeId { get; set; }

        private CountingType countingType;
        public CountingType CountingType
        {
            get => LazyLoader.Load(this, ref countingType);
            set => countingType = value;
        }

        [Display(Name = "Icms p/ Não Contribuinte")]
        public decimal? IcmsNContribuinte { get; set; }

        [Display(Name = "Icms p/ Não Contribuinte Fora do Estado")]
        public decimal? IcmsNContribuinteFora { get; set; }

        [Display(Name = "Tipo")]
        public bool TypeCompany { get; set; }

        [Display(Name = "Icms Aliq. SUperior a 25")]
        public decimal ? IcmsAliqM25 { get; set; }

        [Display(Name = "UF")]
        public string Uf { get; set; }

        [Display(Name = "Inscrição Estadual")]
        public string Ie { get; set; }
    }
}