﻿using System.Collections.Generic;
using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;

namespace Escon.SisctNET.Service.Implementation
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _repository;

        public NoteService(INoteRepository repository)
        {
            _repository = repository;
        }

        public Note Create(Note entity, Log log)
        {
            return _repository.Create(entity, log);
        }

        public List<Note> Create(List<Note> entities, Log log)
        {
            return _repository.Create(entities, log);
        }

        public void Delete(long id, Log log)
        {
            _repository.Delete(id, log);
        }

        public void Delete(List<Note> entities, Log log = null)
        {
            _repository.Delete(entities, log);
        }

        public List<Note> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Note> FindAll(int page, int countrow, Log log)
        {
            return _repository.FindAll(page, countrow, log);
        }

        public List<Note> FindByCompany(long companyId, Log log = null)
        {
            return _repository.FindByCompany(companyId, log);
        }

        public Note FindById(long id, Log log)
        {
            return _repository.FindById(id, log);
        }

        public Note FindByNote(string chave, Log log = null)
        {
            return _repository.FindByNote(chave);
        }

        public Note FindByNote(long noteId, Log log = null)
        {
            return _repository.FindByNote(noteId, log);
        }

        public List<Note> FindByNotes(long id, string year, string month, Log log = null)
        {
            return _repository.FindByNotes(id, year, month);
        }

        public List<Note> FindByUf(long companyId, string year, string month, string uf, Log log = null)
        {
            return _repository.FindByUf(companyId, year, month, uf);
        }

        public Note Update(Note entity, Log log)
        {
            return _repository.Update(entity, log);
        }

        public List<Note> Update(List<Note> entities, Log log)
        {
            return _repository.Update(entities, log);
        }

    }
}
