using Escon.SisctNET.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escon.SisctNET.Web.Controllers
{
    public class AccountPlanController : ControllerBaseSisctNET
    {
        private readonly Service.IAccountPlanService _service;
        private readonly Fortes.IAccountPlanService _fortesAccountPlanService;
        private readonly Service.ICompanyService _companyService;
        private readonly Service.IConfigurationService _configurationService;
        private readonly Service.IAccountPlanTypeService _accountPlanTypeService;
        private readonly Service.IAccountPlanService _accountPlanService;

        public AccountPlanController(
             Service.IAccountPlanService accountPlanService,
             Service.IAccountPlanService service,
             Service.ICompanyService companyService,
             Service.IConfigurationService configurationService,
             Service.IAccountPlanTypeService accountPlanTypeService,
             Fortes.IAccountPlanService fortesAccountPlanService,
             Service.IFunctionalityService functionalityService,
             IHttpContextAccessor httpContextAccessor) : base(functionalityService, "AccountPlan")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);

            _service = service;
            _companyService = companyService;
            _fortesAccountPlanService = fortesAccountPlanService;
            _configurationService = configurationService;
            _accountPlanTypeService = accountPlanTypeService;
            _accountPlanService = accountPlanService;
        }

        [HttpGet]
        public IActionResult Sincronize(long companyId)
        {

            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlan")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var configBase = _configurationService.FindByName("DataBaseFortes", null);
                var company = _companyService.FindById(companyId, null);

                var accountPlaFortes = _fortesAccountPlanService.GetAccountPlans(company, configBase.Value);
                var result = _service.FindByCompanyId(companyId);

                List<Model.AccountPlan> newAccounts = new List<Model.AccountPlan>();
                foreach (var ac in accountPlaFortes)
                {
                    var r = result.Where(a => a.Code.Equals(ac.Code)).FirstOrDefault();
                    if (r == null)
                    {
                        newAccounts.Add(ac);
                        result.Add(ac);
                    }
                    else
                    {
                        if (!r.Name.Equals(ac.Name))
                        {
                            r.Name = ac.Name;
                            r.CompanyId = companyId;
                            _accountPlanService.Update(r, GetLog(OccorenceLog.Update));
                        }
                    }
                }

                _service.Create(newAccounts, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index", new { @companyId = companyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Index(long companyId)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlan")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var company = _companyService.FindById(companyId, null);

                SessionManager.SetCompanyInSession(company);

                var compSession = new Model.Company()
                {
                    Updated = company.Updated,
                    Id = company.Id,
                    Active = company.Active,
                    Code = company.Code,
                    Created = company.Created,
                    Document = company.Document,
                    FantasyName = company.FantasyName,
                    SocialName = company.SocialName
                };

                if (company.AccountPlans.Count == 0)
                {
                    var accounts = _accountPlanService.FindByCompanyId(companyId);
                    accounts.ForEach(a =>
                    {
                        var account = new Model.AccountPlan()
                        {
                            AccountPlanTypeId = a.AccountPlanTypeId,
                            Active = a.Active,
                            Analytical = a.Analytical,
                            Code = a.Code,
                            Created = a.Created,
                            Id = a.Id,
                            Name = a.Name,
                            Patrimonial = a.Patrimonial,
                            Reduced = a.Reduced,
                            Updated = a.Updated,
                            AccountPlanType = new Model.AccountPlanType()
                            {
                                Active = a.AccountPlanType.Active,
                                Updated = a.AccountPlanType.Updated,
                                Name = a.AccountPlanType.Name,
                                Id = a.AccountPlanType.Id,
                                Created = a.AccountPlanType.Created
                            }
                        };

                        compSession.AccountPlans.Add(account);
                    });

                }


                var acc = _accountPlanTypeService.FindAll(null).Where(a => a.Active.Equals(true)).ToList();

                List<AccountPlanType> accountPlanTypes = new List<AccountPlanType>();
                accountPlanTypes.Insert(0, new AccountPlanType() { Id = 0, Name = "Selecione um tipo de conta" });

                foreach (var item in acc)
                {
                    accountPlanTypes.Add(new AccountPlanType() { Id = item.Id, Name = item.Name });
                }

                SelectList accountTypes = new SelectList(accountPlanTypes, "Id", "Name", null);
                ViewBag.ListTypes = accountTypes;

                ViewBag.CompanyName = company.SocialName;
                ViewBag.Document = company.Document;
                ViewBag.CompanyId = company.Id;

                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlan")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {

                var r = new Task(() =>
                {
                    var entity = _service.FindById(updateActive.Id, null);
                    entity.Active = updateActive.Active;
                    _service.Update(entity, GetLog(Model.OccorenceLog.Update));

                });

                r.RunSynchronously();

                return Ok(new { requestcode = 200, message = "ok" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateAccountType([FromBody] Model.UpdateAccountType updateAccountType)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("AccountPlan")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                if (updateAccountType.AccountPlanId <= 0)
                {
                    return NotFound(new { requestcode = 401, message = "A Conta informada informada não foi encontrada. Código: " + updateAccountType.AccountPlanId });
                }

                var entity = SessionManager.GetCompanyInSession().AccountPlans.Where(a => a.Id.Equals(updateAccountType.AccountPlanId)).FirstOrDefault();

                var r = new Task(() =>
                {
                    var e = _service.FindById(updateAccountType.AccountPlanId, null);
                    e.AccountPlanTypeId = updateAccountType.AccountTypeId;

                    _service.Update(e, GetLog(Model.OccorenceLog.Update));
                });


                r.RunSynchronously();

                return Ok(new { requestcode = 200, message = entity.AccountPlanType.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { requestcode = 500, message = ex.Message });
            }
        }

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var acc = _service.FindByCompanyId(SessionManager.GetCompanyInSession().Id);

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<AccountPlan> accTmp = new List<AccountPlan>();
                acc.ForEach(s =>
                {
                    s.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.Name);
                    accTmp.Add(s);
                });

                var ids = accTmp.Where(c =>
                    EF.Functions.Like(c.Name, "%" + filter + "%") ||
                    EF.Functions.Like(c.Code, "%" + filter + "%") ||
                    EF.Functions.Like(c.Reduced.ToString(), "%" + filter + "%"))
                .Select(s => s.Id).ToList();

                acc = acc.Where(a => ids.ToArray().Contains(a.Id)).ToList();
            }

            if (lenght <= 0)
            {
                lenght = acc.Count;
            }

            var retData = acc.OrderBy(o => o.Code).Skip(start).Take(lenght).Select(a => new AccountPlan
            {
                Id = a.Id,
                Code = a.Code,
                Name =  a.Name,
                Reduced = a.Reduced,
                Analytical = a.Analytical,
                Patrimonial = a.Patrimonial,
                Active = a.Active,
                AccountPlanTypeId = a.AccountPlanTypeId,
                AccountPlanType = a.AccountPlanType
            });

            return Ok(new
            {
                draw = draw,
                recordsTotal = acc.Count,
                recordsFiltered = acc.Count,
                data = retData

            });
        }

    }
}
