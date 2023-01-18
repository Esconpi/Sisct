namespace Escon.SisctNET.Service
{
    public interface ITaxationTypeService : IServiceBase<Model.TaxationType>
    {
        Model.TaxationType FindByDescription(string description, Model.Log log = null);
    }
}
