namespace Escon.SisctNET.Service
{
    public interface IGroupService : IServiceBase<Model.Group>
    {
        Model.Group FindByDescription(string description, Model.Log log = null);
    }
}
