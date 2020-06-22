using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository.Implementation
{
    public class SuspensionRepository : Repository<Model.Suspension>, ISuspensionRepository
    {
        private readonly ContextDataBase _context;

        public SuspensionRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
