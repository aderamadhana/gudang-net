using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Gudang.Models.Master;
using Gudang.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (file is null || file.Length == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Gagal import data! file kosong"
                    });
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.First();

                var used = ws.RangeUsed();
                if (used == null)
                {
                    return BadRequest("Sheet kosong");
                }

                var rows = used.RowsUsed().Skip(1);
                var roles = new List<MasterRole>();

                foreach (var row in rows)
                {
                    var name = row.Cell(1).GetString();
                    var description = row.Cell(2).GetString();

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
                    {
                        continue;
                    }

                    roles.Add(new MasterRole
                    {
                        RoleName = name.Trim(),
                        RoleDescription = description.Trim(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = 1,
                        Status = true,
                    });
                }

                await context.MasterRoles.AddRangeAsync(roles);
                await context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Berhasil import data!",
                    inserted = roles.Count()
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = "Gagal import data!",
                    error = e.Message,
                });
            }
            
        }
        public class ExportRequest
        {
            public string Status { get; set; }
        }

        [HttpPost]
        public IActionResult Export([FromBody] ExportRequest request)
        {
            bool? status = null;

            if (!string.IsNullOrEmpty(request.Status) &&
                bool.TryParse(request.Status, out var parsed))
            {
                status = parsed;
            }

            var query = context.MasterRoles
                .Where(r => r.DeletedAt == null)
                .AsNoTracking();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var data = query.ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Master Role");

            // Headers
            ws.Cell(1, 1).Value = "Role Name";
            ws.Cell(1, 2).Value = "Role Description";
            ws.Cell(1, 3).Value = "Status";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;

            // Rows
            for (int i = 0; i < data.Count; i++)
            {
                int row = i + 2;
                ws.Cell(row, 1).Value = data[i].RoleName;
                ws.Cell(row, 2).Value = data[i].RoleDescription;
                ws.Cell(row, 3).Value = data[i].Status ? "ENABLE" : "DISABLE";
            }

            ws.Columns().AdjustToContents();

            // Stream the workbook back as a file download
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"MasterRoles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(
                fileContents: stream.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName
            );
        }
    }
}
