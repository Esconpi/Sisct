namespace Escon.SisctNET.Repository
{
    public interface INcmRepository : IRepository<Model.Ncm>
    {
        Model.Ncm FindByCode(string code, Model.Log log = null);
    }
}
