using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gudang.Models.Master
{
    public class MasterRoleModel
    {
        [Required]
        public string RoleName { get; set; } = "";
        [Required]
        public string RoleDescription { get; set; } = "";
    }
}
