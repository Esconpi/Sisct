using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface ICONService
    {
        List<List<string>> GetBalancete(Model.Company company, DateTime inicio, DateTime fim, string connectionString);
    }
}
