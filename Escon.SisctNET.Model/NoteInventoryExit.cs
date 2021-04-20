using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    public class NoteInventoryExit : EntityBase
    {
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

        [DataType(DataType.Date)]
        [Display(Name = "Emissão")]
        public DateTime Dhemi { get; set; }

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

        [Display(Name = "Total Nota")]
        public decimal Vnf { get; set; }

        [Display(Name = "Fornecedor")]
        public string Xnome { get; set; }

        [Display(Name = "IE Substituto Tributário")]
        public string Iest { get; set; }

    }
}
