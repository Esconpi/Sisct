using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;

namespace Escon.SisctNET.Repository.Implementation
{
    public class AliquotRepository : Repository<Model.Aliquot>, IAliquotRepository
    {
        private readonly ContextDataBase _context;

        public AliquotRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Aliquot> aliquots, Log log = null)
        {
            foreach(var c in aliquots)
            {
                _context.Aliquots.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Aliquot FindByUf(string uf, Log log = null)
        {
            var rst = _context.Aliquots.Where(_ => _.StateOrigem.UF.Equals(uf)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Aliquot FindByUf(List<Aliquot> aliquots, DateTime data, string ufOrigem, string ufDestino, Log log = null)
        {
            string dataFomart = data.ToString("yyyy-MM-dd");
            Aliquot result = null;
            var statesAll = aliquots.Where(_ => _.StateOrigem.UF.Equals(ufOrigem) && _.StateDestino.UF.Equals(ufDestino)).ToList();

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
