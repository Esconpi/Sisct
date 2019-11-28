using System;
using System.Collections.Generic;
using System.Text;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DarRepository : Repository<Dar>, IDarRepository
    {
        private readonly ContextDataBase _context;

        public DarRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

    }
}
