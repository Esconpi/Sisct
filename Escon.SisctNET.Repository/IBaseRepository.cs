namespace Escon.SisctNET.Repository
{
    public interface IBaseRepository : IRepository<Model.Base>
    {
        Model.Base FindByName(string name, Model.Log log = null);
    }
}
