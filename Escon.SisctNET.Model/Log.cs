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
        public int Id { get; set; }

        [ForeignKey("Functionality")]
        public int FunctionalityId { get; set; }

        private Functionality functionality;
        public Functionality Functionality
        {
            get => LazyLoader.Load(this, ref functionality);
            set => functionality = value;
        }

        [ForeignKey("Person")]
        public int PersonId { get; set; }

        private Person person;
        public Person Person
        {
            get => LazyLoader.Load(this, ref person);
            set => person = value;
        }

        [ForeignKey("Occurrence")]
        public int Occurrenceid { get; set; }

        private Occurrence occurrence;
        public Occurrence Occurrence
        {
            get => LazyLoader.Load(this, ref occurrence);
            set => occurrence = value;
        }

        public DateTime Created { get; set; }
    }
}
