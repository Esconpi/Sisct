
namespace Escon.SisctNET.Repository
{
    public interface ITaxRepository : IRepository<Model.Tax>
    {
        Model.Tax FindByMonth(int company, string mes, string ano, Model.Log log = null);

        Model.Tax FindByMonth(int company, string mes, string ano, string type, Model.Log log = null);
    }
}
