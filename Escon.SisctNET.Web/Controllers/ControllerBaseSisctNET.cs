using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class ControllerBaseSisctNET : Controller
    {
        protected string _functionalityName;

        protected Service.IFunctionalityService _functionalityService;

        public ControllerBaseSisctNET(Service.IFunctionalityService functionalityService, string functionalityName)
        {
            _functionalityService = functionalityService;
            _functionalityName = functionalityName;
        }

        protected Model.Log GetLog(Model.OccorenceLog occorenceLog)
        {
            var lg = new Model.Log()
            {
                Created = DateTime.Now,
                FunctionalityId = _functionalityService.FindByName(_functionalityName).Id,
                Occurrenceid = (int)occorenceLog,
                PersonId = SessionManager.GetPersonInSession().Id
            };

            return lg;
        }
    }
}