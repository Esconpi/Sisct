using System;
using System.Linq;
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
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, null);
                ViewBag.Company = company;
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Suspension entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.DateStart = Convert.ToDateTime(Request.Form["DateStart"]);
                entity.DateEnd = Convert.ToDateTime(Request.Form["DateEnd"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.CompanyId = SessionManager.GetCompanyIdInSession();
                _service.Create(entity, GetLog(OccorenceLog.Create));

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession()});
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindById(id, null);

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Suspension entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                entity.DateStart = Convert.ToDateTime(Request.Form["DateStart"]);
                entity.DateEnd = Convert.ToDateTime(Request.Form["DateEnd"]);
                entity.CompanyId = rst.CompanyId;
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Suspension")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
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

            var suspensionsAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession(), null).OrderByDescending(_ => _.Id).ToList();


            var suspension = from r in suspensionsAll
                            select new
                            {
                                Id = r.Id.ToString(),
                                Inicio = r.DateStart.ToString("dd/MM/yyyy hh:mm:ss"),
                                Fim = r.DateEnd.ToString("dd/MM/yyyy hh:mm:ss")

                            };
            return Ok(new { draw = draw, recordsTotal = suspensionsAll.Count(), recordsFiltered = suspensionsAll.Count(), data = suspension.Skip(start).Take(lenght) });

        }
    }
}
