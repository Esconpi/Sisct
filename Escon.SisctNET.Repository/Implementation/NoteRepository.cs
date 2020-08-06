using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class NoteRepository : Repository<Model.Note>, INoteRepository
    {
        private readonly ContextDataBase _context;

        public NoteRepository(ContextDataBase context, IConfiguration configuration) : base(context, configuration)
        {
            _context = context;
        }

        public void Delete(List<Note> notes, Log log = null)
        {
            foreach (var n in notes)
            {
                _context.Notes.Remove(n);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public Note FindByCompany(string company, Log log = null)
        {
            var rst = _context.Notes.Where(_ => _.Company.Equals(company)).FirstOrDefault();
            AddLog(log);
            return rst;
        }
        public Note FindByNote(string chave, Log log = null)
        {
            var rst = _context.Notes.Where(_ => _.Chave.Equals(chave)).FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Note> FindByNotes(int id, string year, string month, Log log = null)
        {
            var rst = _context.Notes.Where(_ => _.CompanyId.Equals(id) && _.AnoRef.Equals(year) && _.MesRef.Equals(month));
            AddLog(log);
            return rst.ToList();
        }

        public List<Note> FindByUf(int companyId, string year, string month, string uf, Log log = null)
        {
            var rst = _context.Notes.Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month) && _.Uf.Equals(uf));
            AddLog(log);
            return rst.ToList();
        }

        public void Update(List<Note> notes, Log log = null)
        {
            foreach (var n in notes)
            {
                _context.Notes.Update(n);
            }

            AddLog(log);
            _context.SaveChanges();
        }
    }
}
