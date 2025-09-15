using Gudang.Models.Master;
using Gudang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Gudang.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environtment;

        public RoleController(ApplicationDbContext context, IWebHostEnvironment environtment)
        {
            this.context = context;
            this.environtment = environtment;
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
        public IActionResult Create(MasterRoleModel masterRoleModel)
        {
            if (!Regex.IsMatch(masterRoleModel.RoleName, @"^[a-zA-Z0-9\s]+$"))
            {
                ModelState.AddModelError("RoleName", "Role Name cannot use spesial character");
            }

            if (!ModelState.IsValid) {
                return View(masterRoleModel);
            }

            MasterRole role = new MasterRole()
            {
                RoleName = masterRoleModel.RoleName,
                RoleDescription = masterRoleModel.RoleDescription,
                Status = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1
            };

            context.MasterRoles.Add(role);
            context.SaveChanges();

            return RedirectToAction("Index", "Role");
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
