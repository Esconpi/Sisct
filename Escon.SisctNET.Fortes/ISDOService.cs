using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Fortes
{
    public interface ISDOService
    {
        List<List<string>> GetDisponibilidadeFinanceira(List<Model.AccountPlan> accountPlans, Model.Company company, DateTime inicio, DateTime fim, string connectionString);

        List<List<string>> GetDespesasOperacionais(List<Model.AccountPlan> accountPlans, Model.Company company, DateTime inicio, DateTime fim, string connectionString);

        List<List<string>> GetEstoqueMercadorias(List<Model.AccountPlan> accountPlans, Model.Company company, DateTime inicio, DateTime fim, string connectionString);
    }
}
