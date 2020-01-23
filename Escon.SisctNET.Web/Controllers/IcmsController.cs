using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class IcmsController : Controller
    {
        public IcmsController()
        {
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
