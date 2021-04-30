using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface ILogRepository
    {
        List<Model.Log> FindAll(Model.Log log);

        List<Model.Log> FindUser(long userId, Model.Log log);

        List<Model.Log> FindFunctionality(long functionalityId, Model.Log log);
    }
}
