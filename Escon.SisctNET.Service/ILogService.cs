using System.Collections.Generic;

namespace Escon.SisctNET.Service
{
    public interface ILogService
    {
        List<Model.Log> FindAll(Model.Log log);

        List<Model.Log> FindUser(int userId, Model.Log log);

        List<Model.Log> FindFunctionality(int functionalityId, Model.Log log);
    }
}
