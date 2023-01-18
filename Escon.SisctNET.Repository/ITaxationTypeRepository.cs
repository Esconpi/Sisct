namespace Escon.SisctNET.Repository
{
    public interface ITaxationTypeRepository : IRepository<Model.TaxationType>
    {
        Model.TaxationType FindByDescription(string description, Model.Log log = null);
    }
}
