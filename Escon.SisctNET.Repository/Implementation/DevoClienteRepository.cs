﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DevoClienteRepository : Repository<Model.DevoCliente>, IDevoClienteRepository
    {
        private readonly ContextDataBase _context;

        public DevoClienteRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public List<DevoCliente> FindByDevoTax(long taxAnexo, Log log = null)
        {
            var rst = _context.DevoClientes.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
