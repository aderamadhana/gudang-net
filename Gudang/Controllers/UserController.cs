using Gudang.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gudang.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;

        public UserController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var user = context.Users.ToList();
            return View(user);
        }
    }
}
