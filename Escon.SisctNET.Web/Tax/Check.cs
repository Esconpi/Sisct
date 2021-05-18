using System;
using System.Collections.Generic;
using System.Linq;

namespace Escon.SisctNET.Web.Tax
{
    public class Check
    {
        // Verificando Apuração Subbstituição Tributária
        public List<List<string>> ApuracaoST(List<Model.Note> notes, List<Model.ProductNote> productNotes)
        {

            List<List<string>> apuracao = new List<List<string>>();

            var calculation = new Calculation();

            foreach (var note in notes)
            {
                var products = productNotes.Where(_ => _.NoteId.Equals(note.Id)).ToList();

                List<string> nota = new List<string>();


                decimal totalIcmsMvaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsPautaIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsMvaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(false) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsPautaSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Pautado.Equals(true) && _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum()), 2),
                                totalIcmsFreteIE = 0, totalFecop1FreteIE = 0, totalFecop2FreteIE = 0, base1FecopFreteIE = 0, base2FecopFreteIE = 0;

                foreach (var prod in products)
                {
                    if (!prod.Note.Iest.Equals(""))
                    {
                        if (Convert.ToDecimal(prod.AliqInterna) > 0)
                        {
                            decimal valorAgreg = 0;

                            if (prod.Mva != null)
                                valorAgreg = calculation.ValorAgregadoMva(prod.Freterateado, Convert.ToDecimal(prod.Mva));

                            if (prod.BCR != null)
                                valorAgreg = calculation.ValorAgregadoBcr(Convert.ToDecimal(prod.BCR), valorAgreg);

                            if (prod.Fecop != null)
                            {
                                if (Convert.ToDecimal(prod.Fecop).Equals(1))
                                {
                                    base1FecopFreteIE += valorAgreg;
                                    totalFecop1FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                }
                                else
                                {
                                    base2FecopFreteIE += valorAgreg;
                                    totalFecop2FreteIE += calculation.ValorFecop(Convert.ToDecimal(prod.Fecop), valorAgreg);
                                }

                            }
                            totalIcmsFreteIE += calculation.ValorAgregadoAliqInt(Convert.ToDecimal(prod.AliqInterna), Convert.ToDecimal(prod.Fecop), valorAgreg);
                        }
                    }
                }

                //  ICMS
                decimal totalGeralIcms = 0, icmsGeralStIE = 0, icmsGeralStSIE = 0, totalIcmsIE = 0, totalIcmsSIE = 0, gnrePagaIE = 0,
                    gnrePagaSIE = 0, gnreNPagaSIE = 0, gnreNPagaIE = 0, valorDiefIE = 0, valorDiefSIE = 0, totalIcmsPagoIE = 0, totalIcmsPagoSIE = 0;


                if (note.Iest.Equals(""))
                {
                    gnreNPagaSIE = Math.Round(Convert.ToDecimal(note.GnreNSt), 2);
                    gnrePagaSIE = Math.Round(Convert.ToDecimal(note.GnreSt), 2);
                    totalIcmsPagoSIE = Math.Round(Convert.ToDecimal(note.IcmsSt), 2);
                }
                else
                {
                    gnreNPagaIE = Math.Round(Convert.ToDecimal(note.GnreNSt), 2);
                    gnrePagaIE = Math.Round(Convert.ToDecimal(note.GnreSt), 2);
                    totalIcmsPagoIE = Math.Round(Convert.ToDecimal(note.IcmsSt), 2);
                }

                icmsGeralStIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);
                icmsGeralStSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.IcmsST).Sum()), 2);

                totalIcmsIE = Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());
                totalIcmsSIE = Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("")).Select(_ => _.TotalICMS).Sum());


                totalGeralIcms = totalIcmsIE + totalIcmsSIE;

                valorDiefIE = Convert.ToDecimal(totalIcmsIE - icmsGeralStIE - gnrePagaIE + gnreNPagaIE - totalIcmsFreteIE);
                valorDiefSIE = Convert.ToDecimal(totalIcmsSIE - icmsGeralStSIE - gnrePagaSIE + gnreNPagaSIE + totalIcmsFreteIE);


                //  FECOP
                decimal gnreNPagaFecopSIE = 0, gnrePagaFecop1SIE = 0, gnrePagaFecop2SIE = 0, gnreNPagaFecopIE = 0, gnrePagaFecop1IE = 0, gnrePagaFecop2IE = 0;

                decimal base1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2),
                               base1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.ValorBCR).Sum()), 2),
                               valorbase1IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                               valorbase1SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.TotalFecop).Sum()), 2),
                               base1fecopIE = 0, base1fecopSIE = 0;

                decimal base2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2),
                              base2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.ValorBCR).Sum()), 2),
                              valorbase2IE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                              valorbase2SIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.TotalFecop).Sum()), 2),
                              base2fecopIE = 0, base2fecopSIE = 0;

                if (note.Iest.Equals(""))
                {
                    base1fecopSIE = Math.Round(Convert.ToDecimal(note.Fecop1), 2);
                    base2fecopSIE = Math.Round(Convert.ToDecimal(note.Fecop2), 2);
                    gnreNPagaFecopSIE = Math.Round(Convert.ToDecimal(note.GnreFecop), 2);
                    gnrePagaFecop1SIE = Math.Round(Convert.ToDecimal(note.FecopGnre1), 2);
                    gnrePagaFecop2SIE = Math.Round(Convert.ToDecimal(note.FecopGnre2), 2);
                }
                else
                {
                    base1fecopIE = Math.Round(Convert.ToDecimal(note.Fecop1), 2);
                    base2fecopIE = Math.Round(Convert.ToDecimal(note.Fecop2), 2);
                    gnreNPagaFecopIE = Math.Round(Convert.ToDecimal(note.GnreFecop), 2);
                    gnrePagaFecop1IE = Math.Round(Convert.ToDecimal(note.FecopGnre1), 2);
                    gnrePagaFecop2IE = Math.Round(Convert.ToDecimal(note.FecopGnre2), 2);

                }


                base1SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                base1IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 1).Select(_ => _.Valoragregado).Sum()), 2);
                base2IE += Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);
                base2SIE += Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.Fecop == 2).Select(_ => _.Valoragregado).Sum()), 2);

                decimal totalBaseFecopIE = base1fecopIE + base2fecopIE,
                        totalBaseFecopSIE = base1fecopSIE + base2fecopSIE,
                        baseNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                        baseNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VbcFcpSt).Sum()), 2),
                        baseNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                        baseNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VbcFcpStRet).Sum()), 2),
                        valorNfe1NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                        valorNfe1RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                        valorNfe1NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 1).Select(_ => _.VfcpST).Sum()), 2),
                        valorNfe1RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 1).Select(_ => _.VfcpSTRet).Sum()), 2),
                        baseNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2),
                        baseNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2),
                        baseNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VbcFcpSt).Sum()), 2),
                        baseNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VbcFcpStRet).Sum()), 2),
                        valorNfe2NormalIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                        valorNfe2RetIE = Math.Round(Convert.ToDecimal(products.Where(_ => !_.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),
                        valorNfe2NormalSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPST == 2).Select(_ => _.VfcpST).Sum()), 2),
                        valorNfe2RetSIE = Math.Round(Convert.ToDecimal(products.Where(_ => _.Note.Iest.Equals("") && _.pFCPSTRET == 2).Select(_ => _.VfcpSTRet).Sum()), 2),                      
                        totalGnreFecopIE = gnrePagaFecop1IE + gnrePagaFecop2IE,
                        totalGnreFecopSIE = gnrePagaFecop1SIE + gnrePagaFecop2SIE,
                        totalFecopCalcIE = valorbase1IE + valorbase2IE,
                        totalFecopCalcSIE = valorbase1SIE + valorbase2SIE,
                        totalFecopNfeIE = valorNfe1NormalIE + valorNfe1RetIE + valorNfe2NormalIE + valorNfe2RetIE,
                        totalFecopNfeSIE = valorNfe1NormalSIE + valorNfe1RetSIE + valorNfe2NormalSIE + valorNfe2RetSIE;

                decimal difvalor1IE = valorbase1IE - valorNfe1NormalIE - valorNfe1RetIE - gnrePagaFecop1IE - totalFecop1FreteIE - totalFecop2FreteIE,
                        difvalor1SIE = valorbase1SIE - valorNfe1NormalSIE - valorNfe1RetSIE - gnrePagaFecop1SIE + totalFecop1FreteIE + totalFecop2FreteIE,
                        difvalor2IE = valorbase2IE - valorNfe2NormalIE - valorNfe2RetIE - gnrePagaFecop2IE - totalFecop2FreteIE,
                        difvalor2SIE = valorbase2SIE - valorNfe2NormalSIE - valorNfe2RetSIE - gnrePagaFecop2SIE + totalFecop2FreteIE,
                        diftotalIE = difvalor1IE + difvalor2IE, diftotalSIE = difvalor1SIE + difvalor2SIE;


                // APURAÇÃO
                if(valorDiefIE > 0 && valorDiefSIE > 0 && (difvalor1IE + difvalor2IE) > 0 && (difvalor1SIE + difvalor2SIE) > 0)
                {
                    if (valorDiefIE < totalIcmsPagoIE || valorDiefSIE < totalIcmsPagoSIE || (difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE) ||
                       (difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }   
                else if (valorDiefIE > 0 && valorDiefSIE > 0 && (difvalor1IE + difvalor2IE) > 0)
                {
                    if (valorDiefIE < totalIcmsPagoIE || valorDiefSIE < totalIcmsPagoSIE || (difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefIE > 0 && valorDiefSIE > 0 && (difvalor1SIE + difvalor2SIE) > 0)
                {
                    if (valorDiefIE < totalIcmsPagoIE || valorDiefSIE < totalIcmsPagoSIE || (difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefSIE > 0 && (difvalor1IE + difvalor2IE) > 0 && (difvalor1SIE + difvalor2SIE) > 0)
                {
                    if (valorDiefSIE < totalIcmsPagoSIE || (difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE) ||
                       (difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefIE > 0 && valorDiefSIE > 0)
                {
                    if (valorDiefIE < totalIcmsPagoIE || valorDiefSIE < totalIcmsPagoSIE)
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefSIE > 0 && (difvalor1IE + difvalor2IE) > 0)
                {
                    if (valorDiefSIE < totalIcmsPagoSIE || (difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefSIE > 0 && (difvalor1SIE + difvalor2SIE) > 0)
                {
                    if (valorDiefSIE < totalIcmsPagoSIE ||  (difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if ((difvalor1IE + difvalor2IE) > 0 && (difvalor1SIE + difvalor2SIE) > 0)
                {
                    if ((difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE) || (difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefIE > 0)
                {
                    if (valorDiefIE < totalIcmsPagoIE)
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if (valorDiefSIE > 0)
                {
                    if ( valorDiefSIE < totalIcmsPagoSIE)
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if ((difvalor1IE + difvalor2IE) > 0)
                {
                    if ((difvalor1IE + difvalor2IE) < (base1fecopIE + base2fecopIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
                else if ((difvalor1SIE + difvalor2SIE) > 0)
                {
                    if ((difvalor1SIE + difvalor2SIE) < (base1fecopSIE + base2fecopSIE))
                    {
                        nota.Add(note.Nnf);
                        nota.Add(note.Dhemi.ToString("dd/MM/yyyy"));
                        nota.Add(note.Xnome);
                        nota.Add(note.Cnpj);
                        nota.Add(note.Vnf.ToString());

                        if (note.Iest.Equals(""))
                        {
                            nota.Add(valorDiefSIE.ToString());
                            nota.Add(totalIcmsPagoSIE.ToString());
                            nota.Add(Math.Round(valorDiefSIE - totalIcmsPagoSIE, 2).ToString());

                            nota.Add((difvalor1SIE + difvalor2SIE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1SIE + difvalor2SIE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }
                        else
                        {
                            nota.Add(valorDiefIE.ToString());
                            nota.Add(totalIcmsPagoIE.ToString());
                            nota.Add(Math.Round(valorDiefIE - totalIcmsPagoIE, 2).ToString());

                            nota.Add((difvalor1IE + difvalor2IE).ToString());
                            nota.Add((base1fecopSIE + base2fecopSIE).ToString());
                            nota.Add(Math.Round((difvalor1IE + difvalor2IE) - (base1fecopSIE + base2fecopSIE), 2).ToString());
                        }

                        apuracao.Add(nota);
                    }
                }
            
            }

            return apuracao;
        }
    }
}
