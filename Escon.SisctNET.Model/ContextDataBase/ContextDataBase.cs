using Microsoft.EntityFrameworkCore;

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

        public DbQuery<DarDocumentCompany> DarDocumentCompanies { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Functionality> Functionalities { get; set; }

        public DbSet<Access> Accesses { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Configuration> Configurations { get; set; }

        public DbSet<Occurrence> Occurrences { get; set; }

        public DbSet<CountingType> CountingTypes { get; set; }

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

        public DbSet<Aliquot> Aliquots { get; set; }

        public DbSet<ProductNote> ProductNotes { get; set; }

        public DbSet<CompanyCfop> CompanyCfops { get; set; }

        public DbSet<Dar> Dars { get; set; }

        public DbSet<DarDocument> DarDocuments { get; set; }

        public DbSet<Annex> Annices { get; set; }

        public DbSet<NcmConvenio> NcmConvenios { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<TaxationNcm> TaxationNcms { get; set; }

        public DbSet<ProductIncentivo> ProductIncentivos { get; set; }

        public DbSet<Product1> Product1s { get; set; }

        public DbSet<Suspension> Suspensions { get; set; }

        public DbSet<Section> Sections { get; set; }

        public DbSet<Chapter> Chapters { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<CreditBalance> CreditBalances { get; set; }

        public DbSet<Tax> Taxes { get; set; }

        public DbSet<Grupo> Grupos { get; set; }

        public DbSet<TypeNcm> TypeNcms { get; set; }

        public DbSet<Base> Bases { get; set; }

        public DbSet<Product2> Product2s { get; set; }

        public DbSet<EmailResponsible> EmailResponsible { get; set; }

        public DbSet<TaxAnexo> TaxAnexos { get; set; }

        public DbSet<CompraAnexo> CompraAnexos { get; set; }

        public DbSet<DevoCliente> DevoClientes { get; set; }

        public DbSet<DevoFornecedor> DevoFornecedors { get; set; }

        public DbSet<VendaAnexo> VendaAnexos { get; set; }

        public DbSet<NatReceita> NatReceitas { get; set; }

        public DbSet<Cst> Csts { get; set; }

        public DbSet<Csosn> Csosns { get; set; }

        public DbSet<TaxProducer> TaxProducers { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<County> Counties { get; set; }

        public DbSet<Incentive> Incentives { get; set; }

        public DbSet<ProductNoteInventoryEntry> ProductNoteInventoryEntries { get; set; }

        public DbSet<ProductNoteInventoryExit> ProductNoteInventoryExits { get; set; }

        public DbSet<Estoque> Estoques { get; set; }

        public DbSet<TaxationTypeNcm> TaxationTypeNcms { get; set; }
    }
}
