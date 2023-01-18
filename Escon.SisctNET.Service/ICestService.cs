namespace Escon.SisctNET.Service
{
    public interface ICestService : IServiceBase<Model.Cest>
    {
        Model.Cest FindByCode(string code, Model.Log log = null);
    }
}
