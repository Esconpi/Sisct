using Microsoft.AspNetCore.Mvc;
using System;

namespace Escon.SisctNET.API.Controllers
{
    [Route("api/Hora")]
    [ApiController]
    public class HoraController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(new { data = DateTime.Now.ToShortDateString(), hora = DateTime.Now.ToString("HH:mm:ss ffff") });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }
    }
}
