using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Escon.SisctNET.Web.Controllers
{
    public class ProductController : ControllerBaseSisctNET
    {
        private readonly IProductService _service;
        private readonly IGroupService _groupService;

        public ProductController(
            IProductService service,
            IGroupService groupService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Product")
        {
            _service = service;
            _groupService = groupService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                SessionManager.SetUserIdInSession(7);
                var result = _service.FindAll(GetLog(Model.OccorenceLog.Read));

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.GroupId = new SelectList(_groupService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
            return View();
        }

        [HttpPost]
        public IActionResult Create(Model.Product entity)
        {
            try
            {
                decimal price = Convert.ToDecimal(Request.Form["price"]);
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;
                entity.Price = price;

                var result = _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                ViewBag.GroupId = new SelectList(_groupService.FindAll(GetLog(Model.OccorenceLog.Read)), "Id", "Description", null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Product entity)
        {
            try
            {
                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                result.Updated = DateTime.Now;
                result.DateEnd = entity.DateStart;
                _service.Update(result, GetLog(Model.OccorenceLog.Update));

                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                entity.DateEnd = null;                   
                _service.Create(entity, GetLog(Model.OccorenceLog.Create));
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}