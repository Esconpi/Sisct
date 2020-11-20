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

        public State FindByUf(List<State> states, DateTime data, string ufOrigem, string ufDestino, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            State result = null;
            var statesAll = states.Where(_ => _.UfOrigem.Equals(ufOrigem) && _.UfDestino.Equals(ufDestino)).ToList();

            foreach (var t in statesAll)
            {
                var dataInicial = DateTime.Compare(t.DateStart, data);
                var dataFinal = DateTime.Compare(Convert.ToDateTime(t.DateEnd), data);

                if (dataInicial <= 0 && t.DateEnd == null)
                {
                    result = t;
                    break;
                }
                else if (dataInicial <= 0 && dataFinal > 0)
                {
                    result = t;
                    break;
                }

            }
            AddLog(log);
            return result;
        }
    }
}
