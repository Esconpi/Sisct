namespace Escon.SisctNET.Repository
{
    public interface ICfopRepository : IRepository<Model.Cfop>
    {
        Model.Cfop FindByCode(string code, Model.Log log = null);
    }
}
