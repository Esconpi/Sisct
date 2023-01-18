namespace Escon.SisctNET.Service
{
    public interface INcmService : IServiceBase<Model.Ncm>
    {
        Model.Ncm FindByCode(string code, Model.Log log = null);
    }
}
