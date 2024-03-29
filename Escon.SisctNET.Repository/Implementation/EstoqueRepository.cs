﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class EstoqueRepository : Repository<Model.Estoque>, IEstoqueRepository
    {
        private readonly ContextDataBase _context;

        public EstoqueRepository(
            ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public List<Estoque> FindByCompany(long company, Log log = null)
        {
            var rst = _context.Estoques.Where(_ => _.CompanyId.Equals(company)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
