using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class OutputDropdownList
    {
        public int id { get; set; }

        public string text { get; set; }
    }

    public class CfopTypeController : ControllerBaseSisctNET
    {
        private readonly ICfopTypeService _service;

        public CfopTypeController(
            ICfopTypeService service,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Cfop")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
        }

        public IActionResult GetAll()
        {
            List<OutputDropdownList> dropdown = new List<OutputDropdownList>();

            var cfopTypes = _service.FindAll(null).OrderBy(_ => _.Name).ToList();

            foreach (var item in cfopTypes)
            {
                dropdown.Add(new OutputDropdownList() { id = item.Id, text = item.Name });
            }

            return Ok(new { results = dropdown, pagination = new { more = false } });
        }
    }
}
