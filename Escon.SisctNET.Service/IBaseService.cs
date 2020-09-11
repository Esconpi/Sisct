
namespace Escon.SisctNET.Service
{
    public interface IBaseService : IServiceBase<Model.Base>
    {
        Model.Base FindByName(string name, Model.Log log = null);
    }
}
