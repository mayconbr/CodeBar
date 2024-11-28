using CRMAudax;
using Estoque.Models;
using Estoque.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ZXing;
using ZXing.Common;
using CoreFtp;
using SkiaSharp;

namespace Estoque.Controllers
{
    public class PostController : Controller
    {
        [HttpPost]
        [Route("~/Post")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Arquivo não encontrado!");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var context2 = new FtpClient(new FtpClientConfiguration
                    {
                        Host = "ftp.crm.gpaudax.com.br",
                        Username = "crm",
                        Password = "4PxUDaGZ",
                        Port = 21,
                    }))
                    {
                        string rootPath = "www/uploads/Estoque";
                        try
                        {
                            await context2.LoginAsync();
                            await context2.CreateDirectoryAsync(rootPath);
                            await context2.ChangeWorkingDirectoryAsync(rootPath);

                            DateTime horaAtual = DateTime.Now;
                            string novoNomeArquivo = "prefixo_" + horaAtual.ToString("HHmmss") + "_" + file.FileName;

                            using (var writeStream = await context2.OpenFileWriteStreamAsync(novoNomeArquivo))
                            {
                                await file.CopyToAsync(writeStream);
                            }
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }

                    stream.Position = 0;
                    using (var skStream = new SKManagedStream(stream))
                    {
                        var bitmap = SKBitmap.Decode(skStream);
                        if (bitmap == null)
                        {
                            return BadRequest("Não foi possível decodificar a imagem.");
                        }

                        var barcodeReader = new BarcodeReaderGeneric
                        {
                            AutoRotate = true,
                            Options = new DecodingOptions
                            {
                                PureBarcode = false,
                                TryHarder = true,
                                PossibleFormats = new[] { BarcodeFormat.EAN_13, BarcodeFormat.CODE_128, BarcodeFormat.QR_CODE, BarcodeFormat.CODE_39, BarcodeFormat.CODABAR }
                            }
                        };

                        var results = DecodeBarcodeAtAllAngles(barcodeReader, bitmap);

                        if (results != null && results.Count > 0)
                        {
                            var distinctResults = results
                                .GroupBy(r => r.Text)
                                .Select(g => g.First())
                                .ToList();

                            using (MyDbContext context = new MyDbContext())
                            {
                                foreach (var result in distinctResults)
                                {
                                    context.CodeBars.Add(new TableCodeBar
                                    {
                                        CodeBar = result.Text,
                                        Date = DateTime.Now,
                                    });
                                }
                                context.SaveChanges();
                            }
                            return Ok($"Codes: {string.Join(", ", distinctResults.Select(r => r.Text))}");
                        }
                        else
                        {
                            return BadRequest("Código de Barras não Encontrado!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }

        private List<Result> DecodeBarcodeAtAllAngles(BarcodeReaderGeneric barcodeReader, SKBitmap bitmap)
        {
            List<Result> allResults = new List<Result>();
            int rotation = 5;
            int[] angles = Enumerable.Range(0, 360 / rotation).Select(x => x * rotation).ToArray();

            List<SKBitmap> rotatedBitmaps = new List<SKBitmap>();

            foreach (var angle in angles)
            {
                rotatedBitmaps.Add(RotateImage(bitmap, angle));
            }

            foreach (var rotatedBitmap in rotatedBitmaps)
            {
                var results = barcodeReader.DecodeMultiple(new CustomLuminanceSource(rotatedBitmap));
                if (results != null)
                {
                    allResults.AddRange(results);
                }
            }

            return allResults;
        }

        private SKBitmap RotateImage(SKBitmap bitmap, float angle)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            SKBitmap rotatedBitmap = new SKBitmap(width, height);

            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Translate(width / 2, height / 2);
                canvas.RotateDegrees(angle);
                canvas.Translate(-width / 2, -height / 2);
                canvas.DrawBitmap(bitmap, new SKPoint(0, 0));
            }

            return rotatedBitmap;
        }
    }
}
