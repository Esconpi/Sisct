namespace Escon.SisctNET.Service
{
    public interface ITaxAnexoService : IServiceBase<Model.TaxAnexo>
    {
        Model.TaxAnexo FindByMonth(long company, string mes, string ano, Model.Log log = null);
    }
}
