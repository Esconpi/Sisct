namespace Escon.SisctNET.Repository
{
    public interface IGroupRepository : IRepository<Model.Group>
    {
        Model.Group FindByDescription(string description, Model.Log log = null);

    }
}
