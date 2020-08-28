
namespace Escon.SisctNET.Repository
{
    public interface ITaxRepository : IRepository<Model.Tax>
    {
        Model.Tax FindByMonth(int company, string mes, string ano, Model.Log log = null);
    }
}
