﻿using Escon.SisctNET.Model;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Escon.SisctNET.Web
{
    public static class SessionManager
    {
        public static IHttpContextAccessor _httpContextAccessor;

        public static void SetIHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static void SetUserIdInSession(int userId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("UserSisctNET", userId);
        }

        public static int GetUserIdInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("UserSisctNET");
        }

        public static void SetLoginInSession(Model.Login login)
        {
            _httpContextAccessor.HttpContext.Session.Set<Model.Login>("LoginSisctNET", login);
        }

        public static void SetPersonInSession(Model.Person person)
        {
            _httpContextAccessor.HttpContext.Session.Set<Model.Person>("PersonSisctNET", person);
        }

        public static Model.Login GetLoginInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<Model.Login>("LoginSisctNET");
        }

        public static Model.Person GetPersonInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<Model.Person>("PersonSisctNET");
        }

        public static void SetAccessesInSession(List<Access> accesses)
        {
            _httpContextAccessor.HttpContext.Session.Set<List<Access>>("AccessesSisctNET", accesses);
        }

        public static List<Access> GetAccessesInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<List<Access>>("AccessesSisctNET");
        }

        public static void SetLogoutInSession()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }

        public static void SetCompanyIdInSession(int companyId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("CompanyIdSisctNET", companyId);
        }

        public static int GetCompanyIdInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("CompanyIdSisctNET");
        }

        public static void SetTipoInSession(int tipo)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("TipoSisctNET", tipo);
        }

        public static int GetTipoInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("TipoSisctNET");
        }

        public static void SetYearInSession(string year)
        {
            _httpContextAccessor.HttpContext.Session.Set<string>("YearSisctNET", year);
        }

        public static string GetYearInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<string>("YearSisctNET");
        }
        public static void SetMonthInSession(string month)
        {
            _httpContextAccessor.HttpContext.Session.Set<string>("MonthSisctNET", month);
        }

        public static string GetMonthInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<string>("MonthSisctNET");
        }
    }

}
