
namespace Escon.SisctNET.Service
{
    public interface ITaxationService : IServiceBase<Model.Taxation>
    {
        Model.Taxation FindByCode(string code, Model.Log log = null);

        Model.Taxation FindByCode2(string code2, Model.Log log = null);
    }
}
