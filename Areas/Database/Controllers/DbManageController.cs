using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    public class DbManageController : Controller
    {
        [Area("Database")]
        [Route("/database-manage/[action]")]
        // GET: DbManage
        public ActionResult Index()
        {
            return View();
        }

    }
}
