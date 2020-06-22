using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class SuspensionController : ControllerBaseSisctNET
    {

        private readonly ISuspensionService _service;
        private readonly ICompanyService _companyService;

        public SuspensionController(
            ISuspensionService service,
            ICompanyService companyService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Suspension")
        {
            _service = service;
            _companyService = companyService;
        }

        public IActionResult Index(int id)
        {
            if (!SessionManager.GetCompanyInSession().Equals(11))
            {
                return Unauthorized();
            }

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

            var suspensionsAll = _service.FindAll(null).Where(_ => _.CompanyId.Equals(SessionManager.GetCompanyIdInSession())).ToList();


            var suspension = from r in suspensionsAll
                            select new
                            {
                                Id = r.Id.ToString(),
                                Inicio = r.DateStart,
                                Fim = r.DateEnd

                            };
            return Ok(new { draw = draw, recordsTotal = suspensionsAll.Count(), recordsFiltered = suspensionsAll.Count(), data = suspension.Skip(start).Take(lenght) });

        }
    }
}
