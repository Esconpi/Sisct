using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
                                    }
                                }
                            }
                            else
                            {
                                product.Add(thecurrentcell.CellValue.Text);
                            }
                        }

                        products.Add(product);

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

            return products;
        }
        
        public List<List<string>> Notes(string directoryPlanilha)
        {
            List<List<string>> notes = new List<List<string>>();

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
                                    }
                                }
                            }
                            else
                            {
                                note.Add(thecurrentcell.CellValue.Text);
                            }
                        }

                        notes.Add(note);

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
            return notes;
        }

        public List<List<string>> Ncms(string directoryPlanilha)
        {
            List<List<string>> ncms = new List<List<string>>();

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
                                    }
                                }
                                else
                                {
                                    ncm.Add(thecurrentcell.CellValue.Text);
                                }
                            }
                        }

                        if(ncm.Count() >= 15)
                        {
                            if (!ncm[0].Equals("CFOP") && !ncm[1].Equals("Item/Serviço") && !ncm[2].Equals("NCM"))
                            {
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
            return ncms;
        }

    }
}
