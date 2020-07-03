using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository.Implementation
{
    public class SectionRepository : Repository<Section>, ISectionRepository
    {
        private readonly ContextDataBase _context;

        public SectionRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }
    }
}
