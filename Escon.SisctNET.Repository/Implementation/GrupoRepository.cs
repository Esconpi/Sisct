using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class GrupoRepository : Repository<Model.Grupo>, IGrupoRepository
    {
        private readonly ContextDataBase _context;

        public GrupoRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Grupo> grupos, Log log = null)
        {
            foreach (var g in grupos)
            {
                _context.Grupos.Add(g);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<Grupo> FindByGrupos(long taxid, Log log = null)
        {
            var result = _context.Grupos.Where(_ => _.TaxId.Equals(taxid)).ToList();
            AddLog(log);
            return result;
        }

        public void Update(List<Grupo> grupos, Log log = null)
        {
            foreach (var g in grupos)
            {
                _context.Grupos.Update(g);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
