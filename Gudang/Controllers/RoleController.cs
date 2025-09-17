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

        public IActionResult Edit(int id)
        {
            var roles = context.MasterRoles.Find(id);
            if (roles == null)
            {
                return RedirectToAction("Index", "Role");
            }

            var masterRoleModel = new MasterRoleModel()
            {
                RoleName = roles.RoleName,
                RoleDescription = roles.RoleDescription

            };

            ViewData["RoleId"] = roles.RoleId;

            return View(masterRoleModel);
        }
        [HttpPost]
        public IActionResult Edit(int id, MasterRoleModel masterRoleModel)
        {
            var roles = context.MasterRoles.Find(id);
            if (roles == null)
            {
                return RedirectToAction("Index", "Role");
            }

            if (!Regex.IsMatch(masterRoleModel.RoleName, @"^[a-zA-Z0-9\s]+$"))
            {
                ModelState.AddModelError("RoleName", "Role Name cannot use spesial character");
            }

            if (!ModelState.IsValid)
            {
                ViewData["RoleId"] = roles.RoleId;
                return View(masterRoleModel);
            }

            roles.RoleName = masterRoleModel.RoleName;
            roles.RoleDescription = masterRoleModel.RoleDescription;
            roles.UpdatedAt = DateTime.Now;
            roles.UpdatedBy = 1;

            context.SaveChanges();

            return RedirectToAction("Index", "Role");


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

            // permanent constraints for this endpoint
            var baseQuery = context.MasterRoles
                .Where(r => r.Status == status && r.DeletedAt == null);

            // total BEFORE user search, AFTER permanent constraints
            var recordsTotal = baseQuery.Count();

            // apply search
            var filteredQuery = baseQuery;
            if (!string.IsNullOrWhiteSpace(search))
                filteredQuery = filteredQuery.Where(r =>
                    r.RoleName.Contains(search) || r.RoleDescription.Contains(search));

            var recordsFiltered = filteredQuery.Count();

            // page
            var data = filteredQuery.OrderBy(r => r.RoleId)
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

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var roles = context.MasterRoles.Find(id);
            if (roles == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Gagal hapus data! Id tidak ditemukan"
                });
            }

            roles.DeletedAt = DateTime.Now;
            roles.DeletedBy = 1;

            context.SaveChanges();

            return Json(new { 
                success = true, 
                message = "Berhasil hapus data!" 
            });

        }
        [HttpPost]
        public IActionResult ChangeStatus(int id, bool status)
        {
            var roles = context.MasterRoles.Find(id);
            if(roles == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Gagal ubah status! Id tidak ditemukan"
                });
            }

            roles.UpdatedAt = DateTime.Now;
            roles.UpdatedBy = 1;
            roles.Status = status;

            context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Berhasil ubah status!"
            });
        }
    }
}
