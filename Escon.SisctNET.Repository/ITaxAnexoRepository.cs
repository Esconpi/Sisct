namespace Escon.SisctNET.Repository
{
    public interface ITaxAnexoRepository : IRepository<Model.TaxAnexo>
    {
        Model.TaxAnexo FindByMonth(long company, string mes, string ano, Model.Log log = null);
    }
}
