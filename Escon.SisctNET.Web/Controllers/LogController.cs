using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class LogController : ControllerBaseSisctNET
    {
        private readonly ILogService _service;
        private readonly IPersonService _personService;

        public LogController(
            ILogService service,
            IPersonService personService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Log")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _personService = personService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Log")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Filter()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Log")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Model.Person> list_users = _personService.FindAll(null);
                SelectList users = new SelectList(list_users, "Id", "FirstName", null);
                ViewBag.UserId = users;
                return PartialView(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult IndexUser(int userId)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Log")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                SessionManager.SetUserIdInSession(userId);
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

            var logAll = _service.FindAll(null);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Log> logs = new List<Model.Log>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                var ids = logAll.Where(c =>
                    c.Person.FirstName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Functionality.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Occurrence.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                logs = logAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var log = from r in logs
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  Person = r.Person.FirstName,
                                  Functionality = r.Functionality.Description,
                                  Occurrence = r.Occurrence.Name,
                                  Date = r.Created.ToString("dd/MM/yyyy hh:mm:ss")
                              };

                return Ok(new { draw = draw, recordsTotal = logs.Count(), recordsFiltered = logs.Count(), data = log.Skip(start).Take(lenght) });

            }
            else
            {
                var log = from r in logAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Person = r.Person.FirstName,
                              Functionality = r.Functionality.Description,
                              Occurrence = r.Occurrence.Name,
                              Date = r.Created.ToString("dd/MM/yyyy hh:mm:ss")
                          };

                return Ok(new { draw = draw, recordsTotal = logAll.Count(), recordsFiltered = logAll.Count(), data = log.Skip(start).Take(lenght) });
            }

        }

        public IActionResult GetAllUser(int draw, int start)
        {

            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var logAll = _service.FindUser(SessionManager.GetUserIdInSession(),null);


            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.Log> logs = new List<Model.Log>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                var ids = logAll.Where(c =>
                    c.Person.FirstName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Functionality.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Occurrence.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                logs = logAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var log = from r in logs
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Person = r.Person.FirstName,
                              Functionality = r.Functionality.Description,
                              Occurrence = r.Occurrence.Name,
                              Date = r.Created.ToString("dd/MM/yyyy hh:mm:ss")
                          };

                return Ok(new { draw = draw, recordsTotal = logs.Count(), recordsFiltered = logs.Count(), data = log.Skip(start).Take(lenght) });

            }
            else
            {
                var log = from r in logAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Person = r.Person.FirstName,
                              Functionality = r.Functionality.Description,
                              Occurrence = r.Occurrence.Name,
                              Date = r.Created.ToString("dd/MM/yyyy hh:mm:ss")
                          };

                return Ok(new { draw = draw, recordsTotal = logAll.Count(), recordsFiltered = logAll.Count(), data = log.Skip(start).Take(lenght) });
            }

        }
    }
}
