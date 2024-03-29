﻿using System.Collections.Generic;

namespace Escon.SisctNET.Repository
{
    public interface IRepository<T>
    {
        T Create(T entity, Model.Log log);

        List<T> Create(List<T> entities, Model.Log log);

        T FindById(long id, Model.Log log);
        
        List<T> FindAll(Model.Log log);
        
        List<T> FindAll(int page, int countrow, Model.Log log);
        
        T Update(T entity, Model.Log log);

        List<T> Update(List<T> entities, Model.Log log);

        void Delete(long id, Model.Log log);

        void Delete(List<T> entities, Model.Log log);
    }
}
