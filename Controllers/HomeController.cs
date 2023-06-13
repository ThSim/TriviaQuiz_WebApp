using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TriviaQuiz_WebApp.Models;
using TriviaQuiz_WebApp.Models.HelperModel;

namespace TriviaQuiz_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            
            try
            {
                if (SessionToken != null && !String.IsNullOrEmpty(SessionToken.token))
                {
                    using (var client = new ApiClient())
                    {
                       SessionToken =  await Task.Run(() => client.ResetTokenAndFlushDB(SessionToken.token)); 
                    }
                }
            }
            catch
            {
                throw new Exception("An error occured while starting the quiz again.");
            }
            return View();
        }


        public static Token SessionToken;
 

        public async Task<IActionResult> StartAsync()
        {
            try
            {
                using (var client = new ApiClient())
                {
                    SessionToken = await client.GetRetrieveToken();
                }

                return RedirectToAction("Index", "Questions", SessionToken);
            }
            catch
            {

                throw new Exception("Failed to create new session");
            }
        }


    }
}