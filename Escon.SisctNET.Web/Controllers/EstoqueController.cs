﻿using Escon.SisctNET.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Controllers
{
    public class EstoqueController : ControllerBaseSisctNET
    {
        private readonly IEstoqueService _service;
        private readonly ICompanyService _companyService;
        private readonly IProductNoteInventoryEntryService _productNoteInventoryEntryService;
        private readonly IProductNoteInventoryExitService _productNoteInventoryExitService;

        public EstoqueController(
            IEstoqueService service,
            ICompanyService companyService,
            IProductNoteInventoryEntryService productNoteInventoryEntryService,
            IProductNoteInventoryExitService productNoteInventoryExitService,
            IFunctionalityService functionalityService,
            IHttpContextAccessor httpContextAccessor) 
            : base(functionalityService, "Company")
        {
            SessionManager.SetIHttpContextAccessor(httpContextAccessor);
            _service = service;
            _companyService = companyService;
            _productNoteInventoryEntryService = productNoteInventoryEntryService;
            _productNoteInventoryExitService = productNoteInventoryExitService;
        }

        public IActionResult Index(long id)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                SessionManager.SetCompanyIdInSession(id);
                var company = _companyService.FindById(id, null);
                ViewBag.Company = company;
                return View(null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult Create()
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }
        }

        public IActionResult Relatory(long id, string year, string month, string type)
        {
            if (SessionManager.GetLoginInSession().Equals(null)) return Unauthorized();

            try
            {
                var comp = _companyService.FindById(id, null);
                ViewBag.Company = comp;
                ViewBag.Tipo = type;

                SessionManager.SetMonthInSession(month);
                SessionManager.SetYearInSession(year);

                var importPeriod = new Period.Month();

                var produtosCompany = _service.FindByCompany(id);
                var entradas = _productNoteInventoryEntryService.FindByNotes(id, year, month);
                var saidas = _productNoteInventoryExitService.FindByNotes(id, year, month);

                var produtosTemp = entradas
                              .Select(_ => _.Cprod)
                              .Distinct()
                              .ToList();

                produtosTemp.AddRange(saidas
                           .Select(_ => _.Cprod)
                           .Distinct()
                           .ToList());

                produtosTemp = produtosTemp
                            .Distinct()
                            .ToList();

                if (type.Equals("historico"))
                {
                    List<List<string>> produtos = new List<List<string>>();
                    List<List<string>> estoque = new List<List<string>>();

                    foreach (var p in produtosTemp)
                    {
                        var ppEntrada = entradas.Where(_ => _.Cprod.Equals(p)).FirstOrDefault();

                        if (ppEntrada == null)
                        {
                            var ppSaida = saidas.Where(_ => _.Cprod.Equals(p)).FirstOrDefault();

                            List<string> produto = new List<string>();
                            produto.Add(ppSaida.Cprod);
                            produto.Add(ppSaida.Xprod);
                            produto.Add(ppSaida.Ucom);
                            produto.Add(ppSaida.Ncm);
                            produto.Add(ppSaida.Cest);

                            produtos.Add(produto);
                        }
                        else
                        {
                            List<string> produto = new List<string>();
                            produto.Add(ppEntrada.Cprod);
                            produto.Add(ppEntrada.Xprod);
                            produto.Add(ppEntrada.Ucom);
                            produto.Add(ppEntrada.Ncm);
                            produto.Add(ppEntrada.Cest);

                            produtos.Add(produto);
                        }
                    }

                    foreach (var produto in produtos)
                    {
                        // DESFAZER
                        decimal quantidade = 1, total = Convert.ToDecimal(24.09), valor = total / quantidade;

                        var primeiroDia = entradas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .FirstOrDefault();

                        if (saidas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .FirstOrDefault() < primeiroDia)
                        {
                            primeiroDia = saidas
                              .OrderBy(_ => _.Dhemi)
                              .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                              .FirstOrDefault();
                        }

                        var ultimoDia = entradas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .LastOrDefault();

                        if (saidas
                           .OrderBy(_ => _.Dhemi)
                           .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                           .LastOrDefault() > primeiroDia)
                        {
                            ultimoDia = saidas
                              .OrderBy(_ => _.Dhemi)
                              .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                              .LastOrDefault();
                        }


                        for (int dia = primeiroDia; dia <= ultimoDia; dia++)
                        {
                            var entradaDia = entradas
                            .Where(_ => Convert.ToInt32(_.Dhemi.ToString("dd")).Equals(dia) && _.Cprod.Equals(produto[0]))
                            .OrderBy(_ => _.Cprod)
                            .ToList();

                            foreach (var entrada in entradaDia)
                            {
                                List<string> prod = new List<string>();

                                prod.Add("COMPRA");
                                prod.Add(entrada.Cprod);
                                prod.Add(entrada.Dhemi.ToString("dd/MM/yyyy"));
                                prod.Add(entrada.Nnf);
                                prod.Add(entrada.Qcom.ToString());
                                prod.Add(entrada.Vuncom.ToString());
                                prod.Add(entrada.Vbasecalc.ToString());

                                quantidade += Convert.ToDecimal(entrada.Qcom);
                                total += Convert.ToDecimal(entrada.Vbasecalc);
                                valor = Math.Round(total / quantidade, 2);
                                estoque.Add(prod);
                            }

                            var saidaDia = saidas
                                .Where(_ => Convert.ToInt32(_.Dhemi.ToString("dd")).Equals(dia) && _.Cprod.Equals(produto[0]))
                                .OrderBy(_ => _.Cprod)
                                .ToList();

                            foreach (var saida in saidaDia)
                            {
                                List<string> prod = new List<string>();

                                prod.Add("VENDA");
                                prod.Add(saida.Cprod);
                                prod.Add(saida.Dhemi.ToString("dd/MM/yyyy"));
                                prod.Add(saida.Nnf);
                                prod.Add(saida.Qcom.ToString());
                                prod.Add(saida.Vuncom.ToString());
                                prod.Add(saida.Vbasecalc.ToString());


                                if (quantidade - Convert.ToDecimal(saida.Qcom) <= 0)
                                {
                                    quantidade = 0;
                                    total = 0;
                                    valor = 0;
                                }
                                else
                                {
                                    quantidade -= Convert.ToDecimal(saida.Qcom);
                                    total -= Convert.ToDecimal(saida.Vbasecalc);
                                    valor = Math.Round(total / quantidade, 2);
                                }

                                estoque.Add(prod);
                            }

                        }

                        produto.Add(quantidade.ToString());
                        produto.Add(valor.ToString());
                        produto.Add(total.ToString());
                    }

                    ViewBag.Produtos = produtos.OrderBy(_ => _[0]).ToList();
                    ViewBag.Estoque = estoque;
                }
                else if (type.Equals("livro"))
                {
                    List<List<string>> produtos = new List<List<string>>();

                    foreach(var p in produtosTemp)
                    {
                        var ppEntrada = entradas.Where(_ => _.Cprod.Equals(p)).FirstOrDefault();

                        if(ppEntrada == null)
                        {
                            var ppSaida = saidas.Where(_ => _.Cprod.Equals(p)).FirstOrDefault();

                            List<string> produto = new List<string>();
                            produto.Add(ppSaida.Cprod);
                            produto.Add(ppSaida.Xprod);
                            produto.Add(ppSaida.Ucom);
                            produto.Add(ppSaida.Ncm);
                            produto.Add(ppSaida.Cest);

                            produtos.Add(produto);
                        }
                        else
                        {
                            List<string> produto = new List<string>();
                            produto.Add(ppEntrada.Cprod);
                            produto.Add(ppEntrada.Xprod);
                            produto.Add(ppEntrada.Ucom);
                            produto.Add(ppEntrada.Ncm);
                            produto.Add(ppEntrada.Cest);

                            produtos.Add(produto);
                        }
                    }
                 
                    foreach (var produto in produtos)
                    {
                        decimal quantidade = produtosCompany.Where(_ => _.Cprod.Equals(produto[0])).FirstOrDefault().Quantity, 
                            total = produtosCompany.Where(_ => _.Cprod.Equals(produto[0])).FirstOrDefault().Total,
                            valor = total / quantidade;

                        var primeiroDia = entradas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .FirstOrDefault();

                        if(saidas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .FirstOrDefault() < primeiroDia)
                        {
                            primeiroDia = saidas
                              .OrderBy(_ => _.Dhemi)
                              .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                              .FirstOrDefault();
                        }

                        var ultimoDia = entradas
                            .OrderBy(_ => _.Dhemi)
                            .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                            .LastOrDefault();

                        if (saidas
                           .OrderBy(_ => _.Dhemi)
                           .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                           .LastOrDefault() > primeiroDia)
                        {
                            ultimoDia = saidas
                              .OrderBy(_ => _.Dhemi)
                              .Select(_ => Convert.ToInt32(_.Dhemi.ToString("dd")))
                              .LastOrDefault();
                        }


                        for (int dia = primeiroDia; dia <= ultimoDia; dia++)
                        {
                            var entradaDia = entradas
                            .Where(_ => Convert.ToInt32(_.Dhemi.ToString("dd")).Equals(dia) && _.Cprod.Equals(produto[0]))
                            .OrderBy(_ => _.Cprod)
                            .ToList();

                            if(entradaDia.Count() > 0)
                            {
                                quantidade += Convert.ToDecimal(entradaDia.Select(_ => _.Qcom).Sum());
                                total += Convert.ToDecimal(entradaDia.Select(_ => _.Vbasecalc).Sum());
                                if (quantidade > 0)
                                    valor = total / quantidade;
                            }

                            var saidaDia = saidas
                                .Where(_ => Convert.ToInt32(_.Dhemi.ToString("dd")).Equals(dia) && _.Cprod.Equals(produto[0]))
                                .OrderBy(_ => _.Cprod)
                                .ToList();


                            if(saidaDia.Count() > 0)
                            {
                                if (quantidade > 0)
                                {
                                    quantidade -= Convert.ToDecimal(saidaDia.Select(_ => _.Qcom).Sum());
                                    total -= valor;

                                    if (quantidade > 0)
                                    {
                                        valor = total / quantidade;
                                    }
                                }
                            }
                                
                        }

                        produto.Add(quantidade.ToString());
                        produto.Add(valor.ToString());
                        produto.Add(total.ToString());
                    }

                    ViewBag.Produtos = produtos.OrderBy(_ => _[0]).ToList();
                }

                return View();
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

            var estoqueAll = _service.FindByCompany(SessionManager.GetCompanyIdInSession(), null).OrderByDescending(_ => _.Id).ToList();


            var estoque = from r in estoqueAll
                             select new
                             {
                                 Id = r.Id.ToString(),
                                 Quantity = r.Quantity,
                                 Value = r.Value,
                                 Total = r.Total

                             };
            return Ok(new { draw = draw, recordsTotal = estoqueAll.Count(), recordsFiltered = estoqueAll.Count(), data = estoque.Skip(start).Take(lenght) });

        }
    }
}
