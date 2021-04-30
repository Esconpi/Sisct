
namespace Escon.SisctNET.Service
{
    public interface ITaxService : IServiceBase<Model.Tax>
    {
        Model.Tax FindByMonth(long company, string mes, string ano, Model.Log log = null);

        Model.Tax FindByMonth(long company, string mes, string ano, string type, Model.Log log = null);
    }
}
