using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class StateRepository : Repository<Model.State>, IStateRepository
    {
        private readonly ContextDataBase _context;

        public StateRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<State> states, Log log = null)
        {
            foreach(var c in states)
            {
                _context.States.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public State FindByUf(string uf, Log log = null)
        {
            var rst = _context.States.Where(_ => _.UfOrigem.Equals(uf)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
    }
}
