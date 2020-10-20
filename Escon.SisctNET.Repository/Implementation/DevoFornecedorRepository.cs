using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class DevoFornecedorRepository : Repository<Model.DevoFornecedor>, IDevoFornecedorRepository
    {
        private readonly ContextDataBase _context;

        public DevoFornecedorRepository(ContextDataBase context, IConfiguration configuration)
            : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<DevoFornecedor> devoFornecedors, Log log = null)
        {
            foreach (var d in devoFornecedors)
            {
                _context.DevoFornecedors.Add(d);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<DevoFornecedor> FindByDevoTax(int taxAnexo, Log log = null)
        {
            var rst = _context.DevoFornecedors.Where(_ => _.TaxAnexoId.Equals(taxAnexo)).ToList();
            AddLog(log);
            return rst;
        }

        public void Update(List<DevoFornecedor> devoFornecedors, Log log = null)
        {
            foreach (var d in devoFornecedors)
            {
                _context.DevoFornecedors.Update(d);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
