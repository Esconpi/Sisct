using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace Escon.SisctNET.Web.Controllers
{
    public class TesteController : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View();
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = 500, message = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult Index(string barcode)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bitMap = new Bitmap(barcode.Length * 20, 80))
                {
                    using (Graphics graphics = Graphics.FromImage(bitMap))
                    {
                        Font oFont = new Font("IDAutomationHC39M", 16);
                        PointF point = new PointF(2f, 2f);
                        SolidBrush blackBrush = new SolidBrush(Color.Black);
                        SolidBrush whiteBrush = new SolidBrush(Color.White);
                        graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                        graphics.DrawString("*" + barcode + "*", oFont, blackBrush, point);
                    }

                    bitMap.Save(ms, ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();
                    
                    ViewBag.BarcodeImage = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                }

                //A imagem é desenhada baseada no tamanho do texto
                /*using (Bitmap bitMap = new Bitmap(barcode.Length * 40, 80))
                {
                    //O objeto Graphics é gerado para a imagem
                    using (Graphics graphics = Graphics.FromImage(bitMap))
                    {
                        //Usamos a fonte Barcode
                        Font oFont = new Font("IDAutomationHC39M", 16);
                        PointF point = new PointF(2f, 2f);
                        //Um objeto White Brush é usado para preencher a imagem com a cor branca
                        SolidBrush whiteBrush = new SolidBrush(Color.White);
                        graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                        //Um objeto Black Brush é usado para desenhar o codigo de barras
                        SolidBrush blackBrush = new SolidBrush(Color.Black);
                        graphics.DrawString(barcode, oFont, blackBrush, point);
                    }

                    Barcode128 code128 = new Barcode128();
                    code128.CodeType = Barcode.CODE128;
                    code128.ChecksumText = true;
                    code128.GenerateChecksum = true;
                    code128.Code = barcode;
                    System.Drawing.Bitmap bm = new System.Drawing.Bitmap(code128.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White));
                    bm.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    //O Bitmap é salvo na Memory Stream.
                    bitMap.Save(ms, ImageFormat.Png);
                    //A imagem é convertida para uma string Base64
                    ViewBag.BarcodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());*/

            }
            return View();
        }
    }
}
