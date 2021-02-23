using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CstRepository : Repository<Model.Cst>, ICstRepository
    {
        private readonly ContextDataBase _context;

        public CstRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Cst> FindByIdent(bool identicador, Log log = null)
        {
            var rst = _context.Csts.Where(_ => _.Ident.Equals(identicador)).ToList();
            AddLog(log);
            return rst;
        }
    }
}
