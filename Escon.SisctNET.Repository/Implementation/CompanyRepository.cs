using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Repository.Implementation
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ContextDataBase _context;

        public CompanyRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Create(List<Company> companies, Model.Log log = null)
        {
            foreach (var c in companies)
            {
                _context.Companies.Add(c);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Company FindByCode(string code, Model.Log log = null)
        {
            var rst = _context.Companies.Where(_ => _.Code.Equals(code)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public Company FindByDocument(string document, Model.Log log = null)
        {
            var rst = _context.Companies.Where(_ => _.Document.Equals(document)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public void Update(List<Company> companies, Log log = null)
        {

        }

        public List<Model.Company> FindByCompanies(Log log = null)
        {
            var rst = _context.Companies
                .Where(_ => _.Active.Equals(true))
                .Include(e => e.EmaiResponsibles)
                .ToList();

            AddLog(log);
            return rst;
        }

        public async Task<List<Company>> ListAllActiveAsync(Log log) => await _context.Companies.Where(x => x.Active).ToListAsync();
    }
}