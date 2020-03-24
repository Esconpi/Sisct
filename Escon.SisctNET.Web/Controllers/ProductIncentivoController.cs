using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Mvc;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductIncentivoController : ControllerBaseSisctNET
    {
        public ProductIncentivoController(
            IFunctionalityService functionalityService)
            : base(functionalityService, "ProducyIncentivo")
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}