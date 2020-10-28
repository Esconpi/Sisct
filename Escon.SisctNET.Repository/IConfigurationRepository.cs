namespace Escon.SisctNET.Repository
{
    public interface IConfigurationRepository : IRepository<Model.Configuration>
    {
        Model.Configuration FindByName(string name, Model.Log log = null);
    }
}
