using Escon.SisctNET.Model;
using Escon.SisctNET.Model.ContextDataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Repository.Implementation
{
    public class Repository<T> : IRepository<T> where T : Model.EntityBase
    {
        private readonly ContextDataBase _context;
        private readonly IConfiguration _configuration;
        private bool EnabledLog = false;

        private DbSet<T> dataSet;

        public Repository(ContextDataBase context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            var logEnabled = _configuration["ConfigurationSisctNET:EnabledLog"];
            if (logEnabled != null)
                EnabledLog = Convert.ToBoolean(logEnabled);

            dataSet = _context.Set<T>();
        }

        public void AddLog(Log log)
        {
            if (!EnabledLog || log == null) return;

            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public T Create(T entity, Model.Log log)
        {
            entity.Created = DateTime.Now;
            entity.Updated = entity.Created;

            dataSet.Add(entity);

            AddLog(log);
            _context.SaveChanges();

            return entity;
        }

        public List<T> Create(List<T> entities, Log log)
        {
            foreach (var entity in entities)
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;

                dataSet.Add(entity);
            }

            AddLog(log);
            _context.SaveChanges();

            return entities;
        }

        public void Delete(long id, Model.Log log)
        {
            var rs = dataSet.SingleOrDefault(i => i.Id.Equals(id));
            if (rs != null)
            {
                dataSet.Remove(rs);
                AddLog(log);
                _context.SaveChanges();
            }
        }

        public void Delete(List<T> entities, Log log)
        {
            foreach (var entity in entities)
            {
                dataSet.Remove(entity);
            }

            AddLog(log);
            _context.SaveChanges();
        }

        public List<T> FindAll(Model.Log log)
        {
            AddLog(log);
            return dataSet.ToList();
        }

        public List<T> FindAll(int page, int countrow, Model.Log log)
        {
            AddLog(log);
            return dataSet.GetPaged(page, countrow).Results.ToList();
        }

        public T FindById(long id, Model.Log log)
        {
            if (log != null)
                AddLog(log);

            return dataSet.SingleOrDefault(i => i.Id.Equals(id));
        }

        public T Update(T entity, Model.Log log)
        {
            var id = entity.Id;
            var ent = FindById(id, null);
            if (ent == null) return null;

            entity.Created = ent.Created;
            entity.Updated = DateTime.Now;
            _context.Entry(ent).CurrentValues.SetValues(entity);
            AddLog(log);
            _context.SaveChanges();

            return ent;
        }

        public List<T> Update(List<T> entities, Log log)
        {
            foreach (var entity in entities)
            {
                entity.Updated = DateTime.Now;

                dataSet.Update(entity);
            }

            AddLog(log);
            _context.SaveChanges();

            return entities;
        }
    }
}
