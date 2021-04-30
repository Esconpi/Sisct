using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ILogService
    {
        List<Model.Log> FindAll(Model.Log log);

        List<Model.Log> FindUser(long userId, Model.Log log);

        List<Model.Log> FindFunctionality(long functionalityId, Model.Log log);
    }
}
