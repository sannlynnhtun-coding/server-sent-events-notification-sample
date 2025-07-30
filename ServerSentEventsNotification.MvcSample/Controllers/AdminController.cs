using Microsoft.AspNetCore.Mvc;

namespace ServerSentEventsNotification.MvcSample.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
