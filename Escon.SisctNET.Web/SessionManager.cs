﻿using Microsoft.AspNetCore.Http;
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

        public static void SetLogoutInSession()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }

        public static void SetProfileInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("ProfileSiscNET", functionalityId);
        }

        public static int GetProfileInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("ProfileSiscNET");
        }

        public static void SetConfigurationInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("ConfigurationSisctNET", functionalityId);
        }

        public static int GetConfigurationInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("ConfigurationSisctNET");
        }

        public static void SetNoteInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("NoteSisctNET", functionalityId);
        }

        public static int GetNoteInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("NoteSisctNET");
        }

        public static void SetProductInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("ProductSisctNET", functionalityId);
        }

        public static int GetProductInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("ProductSisctNET");
        }

        public static void SetAccessInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("AccessSisctNET", functionalityId);
        }

        public static int GetAccessInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("AccessSisctNET");
        }

        public static void SetUserInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("UserSisctNET", functionalityId);
        }

        public static int GetUserInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("UserSisctNET");
        }

        public static void SetHomeInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("HomeSisctNET", functionalityId);
        }

        public static int GetHomeInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("HomeSisctNET");
        }

        public static void SetNcmInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("NcmSisctNET", functionalityId);
        }

        public static int GetNcmInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("NcmSisctNET");
        }

        public static void SetAttachmentInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("AttachmentSisctNET", functionalityId);
        }

        public static int GetAttachmentInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("AttachmentSisctNET");
        }

        public static void SetGroupInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("GroupSisctNET", functionalityId);
        }

        public static int GetGroupInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("GroupSisctNET");
        }

        public static void SetCestInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("CestSisctNET", functionalityId);
        }

        public static int GetCestInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("CestSisctNET");
        }

        public static void SetCompanyInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("CompanySisctNET", functionalityId);
        }

        public static int GetCompanyInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("CompanySisctNET");
        }

        public static void SetCfopInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("CfopSisctNET", functionalityId);
        }

        public static int GetCfopInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("CfopSisctNET");
        }

        public static void SetStateInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("StateSisctNET", functionalityId);
        }

        public static int GetStateInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("StateSisctNET");
        }

        public static void SetTaxationInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("TaxationSisctNET", functionalityId);
        }

        public static int GetTaxationInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("TaxationSisctNET");
        }

        public static void SetProductNoteInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("ProductNoteSisctNET", functionalityId);
        }

        public static int GetProductNoteInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("ProductNoteSisctNET");
        }

        public static void SetCompanyCfopInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("CompanyCfopSisctNET", functionalityId);
        }

        public static int GetCompanyCfopInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("CompanyCfopSisctNET");
        }

        public static void SetDarInSession(int functionalityId)
        {
            _httpContextAccessor.HttpContext.Session.Set<int>("DarSisctNET", functionalityId);
        }

        public static int GetDarInSession()
        {
            return _httpContextAccessor.HttpContext.Session.Get<int>("DarSisctNET");
        }

    }
}
