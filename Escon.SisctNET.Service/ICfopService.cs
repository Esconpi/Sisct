namespace Escon.SisctNET.Service
{
    public interface ICfopService : IServiceBase<Model.Cfop>
    {
        Model.Cfop FindByCode(string code, Model.Log log = null);
    }
}
