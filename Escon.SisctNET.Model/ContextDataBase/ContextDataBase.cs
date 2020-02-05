﻿using Microsoft.EntityFrameworkCore;

namespace Escon.SisctNET.Model.ContextDataBase
{
    public class ContextDataBase : DbContext
    {
        public ContextDataBase()
        {

        }

        public ContextDataBase(DbContextOptions<ContextDataBase> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Functionality> Functionalities { get; set; }

        public DbSet<Access> Accesses { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Configuration> Configurations { get; set; }

        public DbSet<Occurrence> Occurrences { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        public DbSet<Cfop> Cfops { get; set; }

        public DbSet<Cest> Cests { get; set; }

        public DbSet<Ncm> Ncms { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Note> Notes { get; set; }

        public DbSet<Taxation> Taxations { get; set; }

        public DbSet<TaxationType> Taxationtypes { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<ProductNote> ProductNotes { get; set; }

        public DbSet<CompanyCfop> CompanyCfops { get; set; }

        public DbSet<Dar> Dars { get; set; }

        public DbSet<Annex> Annices { get; set; }

        public DbSet<NcmConvenio> NcmConvenios { get; set; }

        public DbSet<Provider> Providers { get; set; }

    }
}
