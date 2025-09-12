using Gudang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gudang.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext context;

        public RoleController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var roles = context.MasterRoles.ToList();
            return View(roles);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult getRoles(bool status) {

            // DataTables params
            var draw = Request.Form["draw"].FirstOrDefault();
            int start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
            int length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "10");
            string search = Request.Form["search[value]"].FirstOrDefault() ?? "";

            // base query
            var q = context.MasterRoles.AsQueryable();


            q = q.Where(r => r.Status == status);

            // simple search
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(r => r.RoleName.Contains(search) || r.RoleDescription.Contains(search));

            var recordsTotal = context.MasterRoles.Count();
            var recordsFiltered = q.Count();

            // page
            var data = q.OrderBy(r => r.RoleId)
                        .Skip(start)
                        .Take(length)
                        .Select(r => new {
                            r.RoleId,
                            r.RoleName,
                            r.RoleDescription,
                            r.Status
                        })
                        .ToList();

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }
    }
}
