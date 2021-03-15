﻿using Escon.SisctNET.Model;
using Escon.SisctNET.Repository;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Service.Implementation
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _repository;

        public LogService(ILogRepository repository)
        {
            _repository = repository;
        }

        public List<Log> FindAll(Log log)
        {
            return _repository.FindAll(log);
        }

        public List<Log> FindFunctionality(int functionalityId, Log log)
        {
            return _repository.FindFunctionality(functionalityId, log);
        }

        public List<Log> FindUser(int userId, Log log)
        {
            return _repository.FindUser(userId, log);
        }
    }
}
