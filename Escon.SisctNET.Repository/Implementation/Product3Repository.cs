using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class Product3Repository : Repository<Product3>, IProduct3Repository
    {
        private readonly ContextDataBase _context;

        public Product3Repository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public List<Product3> FindByGroup(long groupid, Log log = null)
        {
            var rst = _context.Product3s.Where(_ => _.GroupId.Equals(groupid) && _.DateEnd == null).ToList();
            AddLog(log);
            return rst;
        }

        public Product3 FindByProduct(string code, long grupoId, Log log = null)
        {
            var rst = _context.Product3s.Where(_ => _.Code.Equals(code) && _.GroupId.Equals(grupoId) && _.DateEnd == null).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Product3> FindAllInDate(DateTime dateProd, Log log = null)
        {

            var result = _context.Product3s.Where(_ => (DateTime.Compare(_.DateStart, dateProd) <= 0 && _.DateEnd == null) || 
                                                       (DateTime.Compare(_.DateStart, dateProd) <= 0 && DateTime.Compare(Convert.ToDateTime(_.DateEnd), dateProd) > 0))
                                           .ToList();
            AddLog(log);
            return result;
        }

        public List<Product3> FindByAllGroup(Log log = null)
        {
            var rst = _context.Product3s
                .Include(_ => _.Group)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Product3 FindByProduct(long id, Log log = null)
        {
            var rst = _context.Product3s
                .Where(_ => _.Id.Equals(id))
                .Include(g => g.Group)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public async Task CreateRange(List<Product3> products, Log log = null)
        {
            _context.Product3s.AddRange(products);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRange(List<Product3> products, Log log = null)
        {
            _context.Product3s.UpdateRange(products);
            await _context.SaveChangesAsync();
        }
    }
}
