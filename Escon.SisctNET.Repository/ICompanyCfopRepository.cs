
namespace Escon.SisctNET.Repository
{
    public interface ICompanyCfopRepository : IRepository<Model.CompanyCfop>
    {
        Model.CompanyCfop FindByCompanyCfop(int companyId, int cfopId);
    }
}
