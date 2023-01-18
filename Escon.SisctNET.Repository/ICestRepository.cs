namespace Escon.SisctNET.Repository
{
    public interface ICestRepository : IRepository<Model.Cest>
    {
        Model.Cest FindByCode(string code, Model.Log log = null);
    }
}
