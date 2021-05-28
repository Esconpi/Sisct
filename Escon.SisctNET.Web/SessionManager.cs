using Escon.SisctNET.Model;
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

        public static void SetUserIdInSession(long userId)
        {
            _httpContextAccessor.HttpContext.Session.Set<long>("UserSisctNET", userId);
        }

        public static long GetUserIdInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<long>("UserSisctNET");
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

        public static void SetIncitiveInSession(List<Model.Incentive> incentives)
        {
            _httpContextAccessor.HttpContext.Session.Set<List<Model.Incentive>>("IncitiveSisctNET", incentives);
        }

        public static List<Model.Incentive> GetIncitiveInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<List<Model.Incentive>>("IncitiveSisctNET");
        }

        public static void SetNotesInSession(List<Model.Note> notes)
        {
            _httpContextAccessor.HttpContext.Session.Set<List<Model.Note>>("NoteSisctNET", notes);
        }

        public static List<Model.Note> GetNotesInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<List<Model.Note>>("NoteSisctNET");
        }

        public static void SetCompanyIdInSession(long companyId)
        {
            _httpContextAccessor.HttpContext.Session.Set<long>("CompanyIdSisctNET", companyId);
        }

        public static long GetCompanyIdInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<long>("CompanyIdSisctNET");
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

        public static void SetProductsSped(List<List<string>> products)
        {
            _httpContextAccessor.HttpContext.Session.Set<List<List<string>>>("ProductsSpedSisctNET", products);
        }

        public static List<List<string>> GetProductsSped()
        {
            return _httpContextAccessor.HttpContext.Session.Get<List<List<string>>>("ProductsSpedSisctNET");
        }

        public static void SetMin(int tipo)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("MinSisctNET", tipo);
        }

        public static int GetMin()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("MinSisctNET");
        }

        public static void SetMax(int tipo)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("MaxSisctNET", tipo);
        }

        public static int GetMax()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("MaxSisctNET");
        }

    }

}
