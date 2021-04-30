using System;
using System.Collections.Generic;
using System.Linq;
using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Escon.SisctNET.Web.Controllers
{
    public class SectionController : ControllerBaseSisctNET
    {
        private readonly ISectionService _service;
        private readonly IChapterService _chapterService;

        public SectionController(
            ISectionService service,
            IChapterService chapterService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Section")
        {
            _service = service;
            _chapterService = chapterService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Section")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var result = _service.FindAll(null);
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
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Section")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Chapter> listchapterss = _chapterService.FindAll(null);
                foreach (var chapter in listchapterss)
                {
                    chapter.Name = chapter.Name + " - " + chapter.Description;
                }
                listchapterss.Insert(0, new Chapter() { Name = "Nenhum Capítulo selecionado", Id = 0 });
                SelectList chapters = new SelectList(listchapterss, "Id", "Name", null);
                ViewBag.ChapterId = chapters;
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Create(Model.Section entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Section")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                _service.Create(entity, GetLog(Model.OccorenceLog.Create));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Edit(long id)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Section")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                List<Chapter> listchapterss = _chapterService.FindAll(null);
                foreach (var chapter in listchapterss)
                {
                    chapter.Name = chapter.Name + " - " + chapter.Description;
                }
                listchapterss.Insert(0, new Chapter() { Name = "Nenhuma Capítulo selecionada", Id = 0 });
                SelectList chapters = new SelectList(listchapterss, "Id", "Name", null);
                ViewBag.ChapterId = chapters;
                var result = _service.FindById(id, null);
                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(long id, Model.Section entity)
        {
            if (SessionManager.GetAccessesInSession() == null || !SessionManager.GetAccessesInSession().Where(_ => _.Functionality.Name.Equals("Section")).FirstOrDefault().Active)
                return Unauthorized();

            try
            {
                var rst = _service.FindById(id, null);
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                var result = _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }
    }
}
