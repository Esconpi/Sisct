namespace Escon.SisctNET.Service
{
    public interface IFunctionalityService : IServiceBase<Model.Functionality>
    {
        Model.Functionality FindByName(string name, Model.Log log = null);
    }
}