using CoreFtp;
using CRMAudax;
using Estoque.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Dynamic;

namespace Estoque.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Entradas()
        {
            dynamic mymodel = new ExpandoObject();
            mymodel.Codes = GetCodeBars();
            return View(mymodel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/GetCodeBars")]
        public IEnumerable<TableCodeBar> GetCodeBars()
        {
            using (var context = new MyDbContext())
            {
                var Bars = (from CodeBars in context.CodeBars
                            orderby CodeBars.Id descending
                            select CodeBars).ToList();

                return Bars;
            }
        }

        [HttpPost]
        [Route("~/UploadFileContratoSocial")]
        public async System.Threading.Tasks.Task<IActionResult> UploadFileContratoSocial(long IdCedente)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
             .AddJsonFile("appsettings.json")
             .Build();

            var filePath = Path.GetTempFileName();
            foreach (var formFile in Request.Form.Files)
            {
                if (formFile.Length > 0)
                {
                    using (var context2 = new FtpClient(new FtpClientConfiguration
                    {
                        Host = configuration.GetSection("FtpCredentials")["Host"],
                        Username = configuration.GetSection("FtpCredentials")["Username"],
                        Password = configuration.GetSection("FtpCredentials")["Password"],
                        Port = Convert.ToInt16(configuration.GetSection("FtpCredentials")["Port"]),
                    }))
                    {
                        string rootPath = "www/uploads/CRM/Cedente/" + IdCedente + "/ContratoSocial";
                        try
                        {
                            await context2.LoginAsync();
                            await context2.CreateDirectoryAsync(rootPath);
                            await context2.ChangeWorkingDirectoryAsync(rootPath);

                            using (var writeStream = await context2.OpenFileWriteStreamAsync(formFile.FileName))
                            {
                                await formFile.CopyToAsync(writeStream);
                            }

                            //using (var context = new MyDbContext())
                            //{
                            //    var c = context.ContratosSociais.Add(new TableContratoSocialCedente
                            //    {
                            //        ClienteId = IdCedente,
                            //        pathContratoSocialCedente = rootPath,
                            //        tipoContratoSocialCedente = formFile.ContentType,
                            //        nomeContratoSocialCedente = formFile.FileName,
                            //        dataEnvio = DateTime.UtcNow
                            //    }).Entity;
                            //    context.SaveChanges();
                            //}

                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                            throw;
                        }
                    }
                }
            }
            return BadRequest();
        }
    }
}
