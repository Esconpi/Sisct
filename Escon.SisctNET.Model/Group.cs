using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("group")]
    public class Group : EntityBase
    {
        public ILazyLoader LazyLoader { get; set; }

        [Required(ErrorMessage = "Obrigatório!")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Obrigatório")]
        [Display(Name = "Anexo")]
        [ForeignKey("Attachment")]
        public int AttachmentId { get; set; }

        private Attachment attachment;
        public Attachment Attachment
        {
            get => LazyLoader.Load(this, ref attachment);
            set => attachment = value;
        }

        [Display(Name = "Incentivo")]
        public bool Active { get; set; }
    }
}
