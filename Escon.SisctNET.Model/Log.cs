using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Escon.SisctNET.Model
{
    [Table("log")]
    public class Log
    {
        public ILazyLoader LazyLoader { get; set; }

        [Key]
        public long Id { get; set; }

        [Display(Name = "Funcionalidade")]
        [ForeignKey("Functionality")]
        public long FunctionalityId { get; set; }

        private Functionality functionality;
        public Functionality Functionality
        {
            get => LazyLoader.Load(this, ref functionality);
            set => functionality = value;
        }

        [Display(Name = "Usuário")]
        [ForeignKey("Person")]
        public long PersonId { get; set; }

        private Person person;
        public Person Person
        {
            get => LazyLoader.Load(this, ref person);
            set => person = value;
        }

        [Display(Name = "Ocorrência")]
        [ForeignKey("Occurrence")]
        public long Occurrenceid { get; set; }

        private Occurrence occurrence;
        public Occurrence Occurrence
        {
            get => LazyLoader.Load(this, ref occurrence);
            set => occurrence = value;
        }

        [Display(Name = "Data")]
        public DateTime Created { get; set; }
    }
}
