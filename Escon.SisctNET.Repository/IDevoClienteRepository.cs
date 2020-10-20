using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository
{
    public interface IDevoClienteRepository : IRepository<Model.DevoCliente>
    {
        void Create(List<Model.DevoCliente> devoClientes, Model.Log log = null);

        void Update(List<Model.DevoCliente> devoClientes, Model.Log log = null);

        List<Model.DevoCliente> FindByDevoTax(int taxAnexo, Model.Log log = null);
    }
}
