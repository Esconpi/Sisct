using System;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class NotificationController : ControllerBaseSisctNET
    {
        private readonly INotificationService _service;
        private readonly ICompanyService _companyService;

        public NotificationController(
            INotificationService service,
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Notification")
        {
            _service = service;
            _companyService = companyService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Company")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.Document = company.Document;
                ViewBag.Name = company.SocialName;
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var notificationAll = _service.FindAll(null).Where(_ => _.CompanyId.Equals(SessionManager.GetCompanyIdInSession())).ToList();


            var notifications = from r in notificationAll
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 Description = r.Description,
                                 Mes = r.MesRef,
                                 Ano = r.AnoRef,
                                 Percentual = r.Percentual
                             };
            return Ok(new { draw = draw, recordsTotal = notificationAll.Count(), recordsFiltered = notificationAll.Count(), data = notifications.Skip(start).Take(lenght) });

        }
    }
}
