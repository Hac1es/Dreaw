using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/ocr")]
    public class OCRController : ControllerBase
    {
        private static readonly ConcurrentQueue<Request>
        public IActionResult Index()
        {
            return View();
        }
    }
}
