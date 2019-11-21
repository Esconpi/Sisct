namespace Escon.SisctNET.Repository
{
    public interface IFunctionalityRepository : IRepository<Model.Functionality>
    {
        Model.Functionality FindByName(string name, Model.Log log = null);
    }
}