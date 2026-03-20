using Microsoft.AspNetCore.Mvc;

namespace TravelDost_MVC.Controllers
{
    //[Route("Auth")]
    public class AuthController : Controller
    {
        //[Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        //[Route("Register")]
        public IActionResult SignUp()
        {
            return View();
        }
    }
}
