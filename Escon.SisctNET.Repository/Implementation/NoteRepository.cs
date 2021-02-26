﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
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
            var rst = _context.Notes
                .Where(_ => _.Company.Document.Equals(company))
                .Include(c => c.Company)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Note> FindByCompany(int companyId, Log log = null)
        {
            var rst = _context.Notes
                .Where(_ => _.CompanyId.Equals(companyId))
                .Include(c => c.Company)
                .ToList();
            AddLog(log);
            return rst;
        }

        public Note FindByNote(string chave, Log log = null)
        {
            var rst = _context.Notes
                .Where(_ => _.Chave.Equals(chave))
                .Include(c => c.Company)
                .FirstOrDefault();
            AddLog(log);
            return rst;
        }

        public List<Note> FindByNotes(int id, string year, string month, Log log = null)
        {
            var rst = _context.Notes
                .Where(_ => _.CompanyId.Equals(id) && _.AnoRef.Equals(year) && _.MesRef.Equals(month))
                .Include(c => c.Company)
                .ToList();
            AddLog(log);
            return rst.ToList();
        }

        public List<Note> FindByUf(int companyId, string year, string month, string uf, Log log = null)
        {
            var rst = _context.Notes
                .Where(_ => _.CompanyId.Equals(companyId) && _.AnoRef.Equals(year) && _.MesRef.Equals(month) && _.Uf.Equals(uf))
                .Include(c => c.Company)
                .ToList();
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
