using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Escon.SisctNET.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Planilha
{
    public class Import
    {
        public List<List<string>> Products(string directoryPlanilha)
        {
            List<List<string>> products = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);
                try
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> product = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                product.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                product.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                product.Add(item.InnerXml);
                                            }

                                        }
                                    }
                                }
                                else if (thecurrentcell.CellValue != null)
                                {
                                    product.Add(thecurrentcell.CellValue.Text);
                                }
                            }

                            if (product.Count() < 4)
                            {
                                throw new Exception();
                            }

                            products.Add(product);

                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new IndexOutOfRangeException();
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Arquivo Excel tem linha com menos de 4 colunas", ex);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return products;
        }

        public List<List<string>> Notes(string directoryPlanilha)
        {
            List<List<string>> notes = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);
                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> note = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                note.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                note.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                note.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                }
                                else if (thecurrentcell.CellValue != null)
                                {
                                    note.Add(thecurrentcell.CellValue.Text);
                                }
                            }

                            if (note.Count() == 6)
                            {
                                if (note[5].Count() == 44)
                                {
                                    notes.Add(note);
                                }
                            }

                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return notes;
        }

        public List<List<string>> Ncms(string directoryPlanilha)
        {
            List<List<string>> ncms = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);

                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> ncm = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                ncm.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                ncm.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                ncm.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                    else if (thecurrentcell.CellValue != null)
                                    {
                                        ncm.Add(thecurrentcell.CellValue.Text);
                                    }
                                }
                            }

                            if (ncm.Count() >= 15)
                            {
                                if (!ncm[0].Equals("CFOP") && !ncm[1].Equals("Item/Serviço") && !ncm[2].Equals("NCM"))
                                {
                                    bool achou = false;
                                    foreach (var n in ncms)
                                    {
                                        if (n[2].Equals(ncm[2]) && n[3].Equals(ncm[3]))
                                        {
                                            achou = true;
                                            break;
                                        }

                                    }

                                    if (achou == false)
                                        ncms.Add(ncm);

                                }

                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return ncms;
        }

        public List<List<string>> Inventory(string directoryPlanilha)
        {
            List<List<string>> ncms = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);

                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> ncm = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                ncm.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                ncm.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                ncm.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                    else if (thecurrentcell.CellValue != null)
                                    {
                                        ncm.Add(thecurrentcell.CellValue.Text);
                                    }
                                }
                                else if (thecurrentcell.CellValue != null)
                                {
                                    ncm.Add(thecurrentcell.CellValue.Text);
                                }
                            }

                            if (ncm.Count() >= 6)
                            {
                                if(!ncm[0].Equals("CÓDIGO"))
                                    ncms.Add(ncm);
                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return ncms;
        }

        public List<List<string>> NotesFsist(string directoryPlanilha)
        {
            List<List<string>> notes = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);
               
                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> note = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                note.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                note.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                note.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                }
                                else if (thecurrentcell.CellValue != null)
                                {
                                    note.Add(thecurrentcell.CellValue.Text);
                                }
                            }

                            if (note.Count() == 13)
                            {
                                if (note[3].Count() == 44)
                                {
                                    notes.Add(note);
                                }
                            }

                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return notes;
        }

        public List<List<string>> Coupons(string directoryPlanilha)
        {
            List<List<string>> coupons = new List<List<string>>();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);

                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> coupon = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                coupon.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                coupon.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                coupon.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                    else if (thecurrentcell.CellValue != null)
                                    {
                                        coupon.Add(thecurrentcell.CellValue.Text);
                                    }
                                }
                            }


                            coupons.Add(coupon);

                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            return coupons;
        }

        public List<List<List<string>>> Balancete(string directoryPlanilha, List<AccountPlan> accountPlans)
        {
            List<List<List<string>>> balancete = new List<List<List<string>>>();

            List<List<string>> disponibilidadeFinanceira = new List<List<string>>();
            List<List<string>> despesasOperacionais = new List<List<string>>();
            List<List<string>> estoqueMercadorias = new List<List<string>>();

            //  Disponibilidade Financeira
            var accountPlans1 = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Disponibilidade Financeira")).ToList();

            decimal saldoAnteriorCaixa = 0, saldoAtualCaixa = 0, saldoAnteriorBanco = 0, saldoAtualBanco = 0, saldoAnteriorOutras = 0, saldoAtualOutras = 0;

            var contasCaixa = accountPlans1
                .Where(_ => _.AccountPlanType.Name.Equals("Caixa"))
                .Select(_ => _.Code)
                .ToList();

            var contasBanco = accountPlans1
               .Where(_ => _.AccountPlanType.Name.Equals("Banco"))
               .Select(_ => _.Code)
               .ToList();

            var contasOutras = accountPlans1
              .Where(_ => _.AccountPlanType.Name.Equals("Outras"))
              .Select(_ => _.Code)
              .ToList();


            //  Despesas Operacionais
            var accountPlans2 = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Despesas Operacionais")).ToList();

            decimal proLabore = 0, comissao = 0, combustivel = 0, encargo = 0, tFederal = 0, tEstadual = 0, tMunicipal = 0, agua = 0, aluguel = 0,
                servico = 0, seguro = 0, frete = 0, despesas = 0, outrasDespesas = 0;

            var contasProLabore = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Pró-Labore"))
                .Select(_ => _.Code)
                .ToList();

            var contasComissao = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Comissões, Salários, Ordenados"))
                .Select(_ => _.Code)
                .ToList();

            var contasCombustivel = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Combustíveis e Lubrificantes"))
                .Select(_ => _.Code)
                .ToList();

            var contasEncargo = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Encargos Sociais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTFederal = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Federais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTEstadual = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Estaduais"))
                .Select(_ => _.Code)
                .ToList();

            var contasTMunicipal = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Tributos Municipais"))
                .Select(_ =>
                _.Code)
                .ToList();

            var contasAgua = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Água, Luz, Telefone"))
                .Select(_ => _.Code)
                .ToList();

            var contasAluguel = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Alugueis"))
                .Select(_ => _.Code)
                .ToList();

            var contasServico = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Serviços Profissionais"))
                .Select(_ => _.Code)
                .ToList();

            var contasSeguro = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Seguros"))
                .Select(_ => _.Code)
                .ToList();

            var contasFrete = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Fretes e Carretos"))
                .Select(_ => _.Code)
                .ToList();

            var contasDespesas = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Despesas Financeiras"))
                .Select(_ => _.Code)
                .ToList();

            var contasOutrasDespesas = accountPlans2
                .Where(_ => _.AccountPlanType.Name.Equals("Outras Despesas"))
                .Select(_ => _.Code)
                .ToList();

            //  Estoque Mercadorias
            var accountPlans3 = accountPlans.Where(_ => _.AccountPlanType.AccountPlanTypeGroup.Name.Equals("Estoque de Mercadorias")).ToList();

            decimal saldoAnteriorTributada = 0, saldoAtualTributada = 0, saldoAnteriorNTributada = 0, saldoAtualNTributada = 0, 
                saldoAnteriorOutrasMercadorias = 0, saldoAtualOutrasMercadorias = 0;

            var contasTributadas = accountPlans3
                .Where(_ => _.AccountPlanType.Name.Equals("Tributadas"))
                .Select(_ => _.Code)
                .ToList();

            var contasNTributadas = accountPlans3
               .Where(_ => _.AccountPlanType.Name.Equals("Não Tributadas"))
               .Select(_ => _.Code)
               .ToList();

            var contasOutrasMercadorias = accountPlans3
              .Where(_ => _.AccountPlanType.Name.Equals("Outras"))
              .Select(_ => _.Code)
              .ToList();

            try
            {
                SpreadsheetDocument doc = SpreadsheetDocument.Open(directoryPlanilha, false);

                try
                {

                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    foreach (Sheet thesheet in thesheetcollection)
                    {
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                        foreach (Row thecurrentrow in thesheetdata)
                        {
                            List<string> saldo = new List<string>();

                            foreach (Cell thecurrentcell in thecurrentrow)
                            {
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                saldo.Add(item.Text.Text);
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                saldo.Add(item.InnerText);
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                saldo.Add(item.InnerXml);
                                            }
                                        }
                                    }
                                }
                                else if (thecurrentcell.CellValue != null)
                                {
                                    saldo.Add(thecurrentcell.CellValue.Text);
                                }
                            }

                            if (saldo.Count() >= 10)
                            {
                                decimal saldoAntarior = 0, saldoAtual = 0;

                                var vaalidar1  = decimal.TryParse(saldo[2], out saldoAntarior);

                                var vaalidar2 = decimal.TryParse(saldo[7], out saldoAtual);

                                var codigo = saldo[0].Replace(".", "");

                                if (contasCaixa.Contains(codigo))
                                {
                                    saldoAnteriorCaixa += saldoAntarior;
                                    saldoAtualCaixa += saldoAtual;
                                }
                                else if (contasBanco.Contains(codigo))
                                {
                                    saldoAnteriorBanco += saldoAntarior;
                                    saldoAtualBanco += saldoAtual;
                                }
                                else if (contasOutras.Contains(codigo))
                                {
                                    saldoAnteriorOutras += saldoAntarior;
                                    saldoAtualOutras += saldoAtual;
                                }
                                else if (contasProLabore.Contains(codigo))
                                {
                                    proLabore += saldoAtual;
                                }
                                else if (contasComissao.Contains(codigo))
                                {
                                    comissao += saldoAtual;
                                }
                                else if (contasCombustivel.Contains(codigo))
                                {
                                    combustivel += saldoAtual;
                                }
                                else if (contasEncargo.Contains(codigo))
                                {
                                    encargo += saldoAtual;
                                }
                                else if (contasTFederal.Contains(codigo))
                                {
                                    tFederal += saldoAtual;
                                }
                                else if (contasTEstadual.Contains(codigo))
                                {
                                    tEstadual += saldoAtual;
                                }
                                else if (contasTMunicipal.Contains(codigo))
                                {
                                    tMunicipal += saldoAtual;
                                }
                                else if (contasAgua.Contains(codigo))
                                {
                                    agua += saldoAtual;
                                }
                                else if (contasAluguel.Contains(codigo))
                                {
                                    aluguel += saldoAtual;
                                }
                                else if (contasServico.Contains(codigo))
                                {
                                    servico += saldoAtual;
                                }
                                else if (contasSeguro.Contains(codigo))
                                {
                                    seguro += saldoAtual;
                                }
                                else if (contasFrete.Contains(codigo))
                                {
                                    frete += saldoAtual;
                                }
                                else if (contasDespesas.Contains(codigo))
                                {
                                    despesas += saldoAtual;
                                }
                                else if (contasOutras.Contains(codigo))
                                {
                                    outrasDespesas += saldoAtual;
                                }
                                else if (contasTributadas.Contains(codigo))
                                {
                                    saldoAnteriorTributada += saldoAntarior;
                                    saldoAtualTributada += saldoAtual;
                                }
                                else if (contasNTributadas.Contains(codigo))
                                {
                                    saldoAnteriorNTributada += saldoAntarior;
                                    saldoAtualNTributada += saldoAtual;
                                }
                                else if (contasOutras.Contains(codigo))
                                {
                                    saldoAnteriorOutrasMercadorias += saldoAntarior;
                                    saldoAtualOutrasMercadorias += saldoAtual;
                                }
                            }

                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Arquivo Excel Corrompido", ex);
            }

            //  Disponibilidade Financeira
            List<string> saldoCaixa = new List<string>();
            saldoCaixa.Add("Caixa");
            saldoCaixa.Add(saldoAnteriorCaixa.ToString());
            saldoCaixa.Add(saldoAtualCaixa.ToString());
            disponibilidadeFinanceira.Add(saldoCaixa);

            List<string> saldoBanco = new List<string>();
            saldoBanco.Add("Banco");
            saldoBanco.Add(saldoAnteriorBanco.ToString());
            saldoBanco.Add(saldoAtualBanco.ToString());
            disponibilidadeFinanceira.Add(saldoBanco);

            List<string> saldoOutras = new List<string>();
            saldoOutras.Add("Outras");
            saldoOutras.Add(saldoAnteriorOutras.ToString());
            saldoOutras.Add(saldoAtualOutras.ToString());
            disponibilidadeFinanceira.Add(saldoOutras);

            //  Despesas Operacionais
            List<string> saldoProLabore = new List<string>();
            saldoProLabore.Add("01 - Pró-Labore");
            saldoProLabore.Add(proLabore.ToString());
            despesasOperacionais.Add(saldoProLabore);

            List<string> saldoComissao = new List<string>();
            saldoComissao.Add("02 - Comissões, Salários, Ordenadose");
            saldoComissao.Add(comissao.ToString());
            despesasOperacionais.Add(saldoComissao);

            List<string> saldoCombustivel = new List<string>();
            saldoCombustivel.Add("03 - Combustíveis e Lubrificantee");
            saldoCombustivel.Add(combustivel.ToString());
            despesasOperacionais.Add(saldoCombustivel);

            List<string> saldoEncargo = new List<string>();
            saldoEncargo.Add("04 - Encargos Sociaise");
            saldoEncargo.Add(encargo.ToString());
            despesasOperacionais.Add(saldoEncargo);

            List<string> saldoTFederal = new List<string>();
            saldoTFederal.Add("05 - Tributos Federais");
            saldoTFederal.Add(tFederal.ToString());
            despesasOperacionais.Add(saldoTFederal);

            List<string> saldoTEstadual = new List<string>();
            saldoTEstadual.Add("06 - Tributos Estaduais");
            saldoTEstadual.Add(tEstadual.ToString());
            despesasOperacionais.Add(saldoTEstadual);

            List<string> saldoTMunicipal = new List<string>();
            saldoTMunicipal.Add("07 - Tributos Municipais");
            saldoTMunicipal.Add(tMunicipal.ToString());
            despesasOperacionais.Add(saldoTMunicipal);

            List<string> saldoAgua = new List<string>();
            saldoAgua.Add("08 - Água, Luz, Telefone");
            saldoAgua.Add(agua.ToString());
            despesasOperacionais.Add(saldoAgua);

            List<string> saldoAluguel = new List<string>();
            saldoAluguel.Add("09 - Alugueis");
            saldoAluguel.Add(aluguel.ToString());
            despesasOperacionais.Add(saldoAluguel);

            List<string> saldoServico = new List<string>();
            saldoServico.Add("10 - Serviços Profissionais");
            saldoServico.Add(servico.ToString());
            despesasOperacionais.Add(saldoServico);

            List<string> saldoSeguro = new List<string>();
            saldoSeguro.Add("11 - Seguros");
            saldoSeguro.Add(seguro.ToString());
            despesasOperacionais.Add(saldoSeguro);

            List<string> saldoFrete = new List<string>();
            saldoFrete.Add("12 - Fretes e Carretos");
            saldoFrete.Add(frete.ToString());
            despesasOperacionais.Add(saldoFrete);

            List<string> saldoDespesas = new List<string>();
            saldoDespesas.Add("13 - Despesas Financeiras");
            saldoDespesas.Add(despesas.ToString());
            despesasOperacionais.Add(saldoDespesas);

            List<string> saldoOutrasDespesas = new List<string>();
            saldoOutrasDespesas.Add("14 - Outras Despesas");
            saldoOutrasDespesas.Add(outrasDespesas.ToString());
            despesasOperacionais.Add(saldoOutrasDespesas);

            //  Estoque Mercadorias
            List<string> saldoTributadas = new List<string>();
            saldoTributadas.Add("Tributadas");
            saldoTributadas.Add(saldoAnteriorTributada.ToString());
            saldoTributadas.Add(saldoAtualTributada.ToString());
            estoqueMercadorias.Add(saldoTributadas);

            List<string> saldoNTributadas = new List<string>();
            saldoNTributadas.Add("Não Tributadas");
            saldoNTributadas.Add(saldoAnteriorNTributada.ToString());
            saldoNTributadas.Add(saldoAtualNTributada.ToString());
            estoqueMercadorias.Add(saldoNTributadas);

            List<string> saldoOutrasMercadorias = new List<string>();
            saldoOutrasMercadorias.Add("Outras");
            saldoOutrasMercadorias.Add(saldoAnteriorOutrasMercadorias.ToString());
            saldoOutrasMercadorias.Add(saldoAtualOutrasMercadorias.ToString());
            estoqueMercadorias.Add(saldoOutrasMercadorias);

            balancete.Add(disponibilidadeFinanceira);
            balancete.Add(despesasOperacionais);
            balancete.Add(estoqueMercadorias);

            return balancete;
        }
    }
}
