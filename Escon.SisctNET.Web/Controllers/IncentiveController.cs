using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class IncentiveController : ControllerBaseSisctNET
    {
        private readonly IIncentiveService _service;
        private readonly ICompanyService _companyService;
        private readonly IConfigurationService _configurationService;

        public IncentiveController(
            IIncentiveService service,
            ICompanyService companyService,
            IConfigurationService configurationService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Notification")
        {
            _service = service;
            _companyService = companyService;
            _configurationService = configurationService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, GetLog(Model.OccorenceLog.Read));
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
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

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
        public IActionResult Create(Model.Incentive entity)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var incentive = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).Where(_ => _.Active.Equals(true)).FirstOrDefault();

                if(incentive != null)
                {
                    incentive.Active = false;
                    incentive.Updated = DateTime.Now;
                    _service.Update(incentive, null);
                }

                entity.Active = true;
                entity.DateStart = Convert.ToDateTime(Request.Form["DateStart"]);
                entity.DateEnd = Convert.ToDateTime(Request.Form["DateEnd"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.CompanyId = SessionManager.GetCompanyIdInSession();
                _service.Create(entity, null);

                var configMin = _configurationService.FindByName("DiasAvisoMínimoIncentivo");
                var incentives = _service.FindByPeriod(Convert.ToInt32(configMin.Value));
                SessionManager.SetIncitiveInSession(incentives);

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Incentive entity)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.DateStart = Convert.ToDateTime(Request.Form["DateStart"]);
                entity.DateEnd = Convert.ToDateTime(Request.Form["DateEnd"]);
                entity.CompanyId = rst.CompanyId;
                entity.Active = rst.Active;
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                _service.Update(entity, GetLog(Model.OccorenceLog.Update));

                var configMin = _configurationService.FindByName("DiasAvisoMínimoIncentivo");
                var incentives = _service.FindByPeriod(Convert.ToInt32(configMin.Value));
                SessionManager.SetIncitiveInSession(incentives);

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));

                var configMin = _configurationService.FindByName("DiasAvisoMínimoIncentivo");
                var incentives = _service.FindByPeriod(Convert.ToInt32(configMin.Value));
                SessionManager.SetIncitiveInSession(incentives);

                return RedirectToAction("Index", new { id = SessionManager.GetCompanyIdInSession() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Notification(string value)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var companies = _companyService.FindByCompanies(null);
                var incentives = _service.FindByPeriod(SessionManager.GetMin());
                if (value == "Min")
                    incentives = incentives.Where(_ => (_.DateEnd.Subtract(DateTime.Now)).Days <= SessionManager.GetMin() && (_.DateEnd.Subtract(DateTime.Now)).Days > @SessionManager.GetMax()).ToList();
                else
                    incentives = incentives.Where(_ => (_.DateEnd.Subtract(DateTime.Now)).Days <= @SessionManager.GetMax()).ToList();

                ViewBag.Value = value;
                ViewBag.Incentives = incentives;
                ViewBag.Companies = companies;
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var incentivesAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession()).OrderByDescending(_ => _.Id).ToList();


            var incentive = from r in incentivesAll
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 Inicio = r.DateStart.ToString("dd/MM/yyyy hh:mm:ss"),
                                 Fim = r.DateEnd.ToString("dd/MM/yyyy hh:mm:ss"),
                                 Active = r.Active

                             };
            return Ok(new { draw = draw, recordsTotal = incentivesAll.Count(), recordsFiltered = incentivesAll.Count(), data = incentive.Skip(start).Take(lenght) });

        }
    }
}
