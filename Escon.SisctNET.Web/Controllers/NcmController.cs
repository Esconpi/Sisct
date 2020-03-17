using Escon.SisctNET.Model;
using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class NcmController : ControllerBaseSisctNET
    {
        private readonly INcmService _service;
        private readonly ICstService _cstService;

        public NcmController(
            INcmService service,
            ICstService cstService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor)
            : base(functionalityService, "Ncm")
        {
            _service = service;
            _cstService = cstService;
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
        }


        [HttpGet]
        public IActionResult Index(int page = 1)
        {
            try
            {
                var login = SessionManager.GetLoginInSession();

                if (login == null)
                {
                    return RedirectToAction("Index", "Authentication");
                }
                else
                {
                    var rst = _service.FindAll(GetLog(Model.OccorenceLog.Read));

                    return View(null);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false)).ToList();
                list_cstE.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Code", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true)).ToList();
                list_cstS.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Code", null);
                ViewBag.CstSaidaID = cstS;

                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public IActionResult Create(Model.Ncm entity)
        {
            try
            {
                entity.Created = DateTime.Now;
                entity.Updated = entity.Created;
                if (entity.CstEntradaId.Equals(0))
                {
                    entity.CstEntradaId = null;
                }
                if (entity.CstSaidaId.Equals(0))
                {
                    entity.CstSaidaId = null;
                }
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
                List<Model.Cst> list_cstE = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(false)).ToList();
                list_cstE.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstE = new SelectList(list_cstE, "Id", "Code", null);
                ViewBag.CstEntradaId = cstE;

                List<Model.Cst> list_cstS = _cstService.FindAll(GetLog(Model.OccorenceLog.Read)).Where(_ => _.Ident.Equals(true)).ToList();
                list_cstS.Insert(0, new Model.Cst() { Code = "Nennhum", Id = 0 });
                SelectList cstS = new SelectList(list_cstS, "Id", "Code", null);
                ViewBag.CstSaidaID = cstS;

                var result = _service.FindById(id, GetLog(Model.OccorenceLog.Read));

                if (result.CstSaidaId == null)
                {
                    result.CstSaidaId = 0;
                }
                if (result.CstEntradaId == null)
                {
                    result.CstEntradaId = 0;
                }

                if (result.CstSaidaRealId == null)
                {
                    result.CstSaidaRealId = 0;
                }
                if (result.CstEntradaRealId == null)
                {
                    result.CstEntradaRealId = 0;
                }

                return View(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Edit(int id, Model.Ncm entity)
        {
            try
            {
                var rst = _service.FindById(id, GetLog(Model.OccorenceLog.Read));
                entity.Created = rst.Created;
                entity.Updated = DateTime.Now;
                if (entity.CstEntradaId.Equals(0))
                {
                    entity.CstEntradaId = null;
                }
                if (entity.CstSaidaId.Equals(0))
                {
                    entity.CstSaidaId = null;
                }

                if (entity.CstEntradaRealId.Equals(0))
                {
                    entity.CstEntradaRealId = null;
                }
                if (entity.CstSaidaRealId.Equals(0))
                {
                    entity.CstSaidaRealId = null;
                }

                bool status = false;
                if (!entity.CstEntradaId.Equals(0) && !entity.CstSaidaId.Equals(0))
                {
                    status = true;
                }
                entity.Status = status;

                bool statuReal = false;
                if (!entity.CstEntradaRealId.Equals(0) && !entity.CstSaidaRealId.Equals(0))
                {
                    statuReal = true;
                }
                entity.StatusReal = statuReal;

                var result = _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id, GetLog(Model.OccorenceLog.Delete));
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus([FromBody] Model.UpdateActive updateActive)
        {
            try
            {
                var entity = _service.FindById(updateActive.Id, GetLog(Model.OccorenceLog.Read));
                entity.Active = updateActive.Active;

                _service.Update(entity, GetLog(Model.OccorenceLog.Update));
                return Ok(new { requestcode = 200, message = "ok" });
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

            var ncmsAll = _service.FindAll(GetLog(Model.OccorenceLog.Read)).OrderBy(_ => _.Code);



            if (!string.IsNullOrEmpty(Request.Query["search[value]"]))
            {
                List<Ncm> ncms = new List<Ncm>();

                var filter = Helpers.CharacterEspecials.RemoveDiacritics(Request.Query["search[value]"].ToString());

                List<Ncm> ncmTemp = new List<Ncm>();
                ncmsAll.ToList().ForEach(s =>
                {
                    s.Description = Helpers.CharacterEspecials.RemoveDiacritics(s.Description);
                    s.Code = s.Code;
                    ncmTemp.Add(s);
                });

                var ids = ncmTemp.Where(c =>
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id).ToList();

                ncms = ncmsAll.Where(a => ids.ToArray().Contains(a.Id)).ToList();

                var ncm = from r in ncms
                          where ids.ToArray().Contains(r.Id)
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };

                return Ok(new { draw = draw, recordsTotal = ncms.Count(), recordsFiltered = ncms.Count(), data = ncm.Skip(start).Take(lenght) });

            }
            else
            {


                var ncm = from r in ncmsAll
                          select new
                          {
                              Id = r.Id.ToString(),
                              Code = r.Code,
                              Description = r.Description

                          };
                return Ok(new { draw = draw, recordsTotal = ncmsAll.Count(), recordsFiltered = ncmsAll.Count(), data = ncm.Skip(start).Take(lenght) });
            }

        }
    }
}