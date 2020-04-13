using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Escon.SisctNET.Repository.Implementation
{
    public class TypeClientRepository : Repository<Model.TypeClient>, ITypeClientRepository
    {
        private readonly ContextDataBase _context;

        public TypeClientRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }
    }
}
