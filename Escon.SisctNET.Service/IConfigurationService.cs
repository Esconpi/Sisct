namespace Escon.SisctNET.Service
{
    public interface IConfigurationService : IServiceBase<Model.Configuration>
    {
        Model.Configuration FindByName(string name, Model.Log log = null);
    }
}
