using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly GridFSServiceAndDataBaseService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, GridFSServiceAndDataBaseService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(UserImage file)
        {

            if (ModelState.IsValid) { 
                //Console.WriteLine("UploadFile");
                if (file != null)
                {
                    string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                    /*Console.WriteLine("Not Null");
                    Console.WriteLine(file.Description);
                    Console.WriteLine(file.image.Length);
                    Console.WriteLine(file.image);
                    Console.WriteLine(file.image.FileName);
                    Console.WriteLine(file.image.ContentType);*/
                    var fileId=await _userService.UploadAsync(file.image);
                    

                    var userUploadFile = new BsonDocument{
                        {"file_id",fileId},
                        {"Description",file.Description},
                        {"IP",ip},
                        {"fileName",file.image.FileName},
                    };
                    await _userService.Create(userUploadFile);
                }
                //console
                

            }

            return View("Index");
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var listOfUser=await _userService.Get(ip);
            //Console.WriteLine($"{listOfUser.Count} users");
            //List<IFormFile> files;
            List<ImagePassToViewData> termsList = new List<ImagePassToViewData>();
            foreach (var user in listOfUser)
            {
                string iptemp=user["file_id"].ToString();
                var ipNew=new ObjectId(iptemp);
                //Console.WriteLine($"{ipNew}+ Hi ");
                /*string fileName = user["fileName"].ToString();
                Console.WriteLine(fileName);*/
                var bytes=await _userService.GetFileByNameAsync(ipNew);
                //Console.WriteLine(bytes.Length);
                string imreBase64Data = Convert.ToBase64String(bytes);
                string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                //Passing image data in viewbag to view
                var imageData = new ImagePassToViewData()
                {
                    description = user["Description"].ToString(),
                    data = imgDataURL,
                };
                termsList.Add(imageData);
            //ViewBag.ImageData = imgDataURL;
            }
            ImagePassToViewData[] terms = termsList.ToArray();
            return View(terms);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}