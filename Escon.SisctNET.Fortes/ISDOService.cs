using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface ISDOService
    {
        List<List<string>> GetBalancete(Model.Company company, DateTime inicio, DateTime fim, string connectionString);

        List<List<string>> GetDisponibilidadeFinanceira(Model.Company company, DateTime inicio, DateTime fim, string connectionString);
    }
}
