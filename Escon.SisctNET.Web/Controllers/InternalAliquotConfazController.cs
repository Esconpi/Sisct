using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Escon.SisctNET.Web.Controllers
{
    public class InternalAliquotConfazController : ControllerBaseSisctNET
    {
        private readonly IInternalAliquotConfazService _service;
        private readonly IStateService _stateService;
        private readonly IAnnexService _annexService;

        public InternalAliquotConfazController(
            IInternalAliquotConfazService service,
            IStateService stateService,
            IAnnexService annexService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "InternalAliquot")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _stateService = stateService;
            _annexService = annexService;
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("InternalAliquot")).FirstOrDefault().Active)
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

        public IActionResult GetAll(int draw, int start)
        {
            var query = System.Net.WebUtility.UrlDecode(Request.QueryString.ToString()).Split('&');
            var lenght = Convert.ToInt32(Request.Query["length"].ToString());

            var aliquotAll = _service.FindByAllState(null);

            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Model.InternalAliquotConfaz> aliquots = new List<Model.InternalAliquotConfaz>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Model.InternalAliquotConfaz> aliquotTemp = new List<Model.InternalAliquotConfaz>();
                aliquotAll.ToList().ForEach(s =>
                {
                    s.State.Name = Helpers.CharacterEspecials.RemoveDiacritics(s.State.Name);
                    s.Annex.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Annex.Description);
                    aliquotTemp.Add(s);
                });

                var ids = aliquotTemp.Where(c =>
                    c.State.UF.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.State.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Annex.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                aliquots = aliquotAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var aliquot = from r in aliquots
                              where ids.ToArray().Contains(r.Id)
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  State = r.State.Name + "-" + r.State.UF,
                                  Annex = r.Annex.Description + "-" + r.Annex.Convenio,
                                  Aliquota = r.Aliquota,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                              };

                return Ok(new { draw = draw, recordsTotal = aliquots.Count(), recordsFiltered = aliquots.Count(), data = aliquot.Skip(start).Take(lenght) });

            }
            else
            {

                var aliquot = from r in aliquotAll
                              select new
                              {
                                  Id = r.Id.ToString(),
                                  State = r.State.Name + "-" + r.State.UF,
                                  Annex = r.Annex.Description + "-" + r.Annex.Convenio,
                                  Aliquota = r.Aliquota,
                                  Inicio = r.DateStart.ToString("dd/MM/yyyy"),
                                  Fim = Convert.ToDateTime(r.DateEnd).ToString("dd/MM/yyyy")
                              };
                return Ok(new { draw = draw, recordsTotal = aliquotAll.Count(), recordsFiltered = aliquotAll.Count(), data = aliquot.Skip(start).Take(lenght) });
            }

        }
    }
}
