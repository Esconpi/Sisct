using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ILogRepository
    {
        List<Model.Log> FindAll(Model.Log log);

        List<Model.Log> FindUser(int userId, Model.Log log);

        List<Model.Log> FindFunctionality(int functionalityId, Model.Log log);
    }
}
