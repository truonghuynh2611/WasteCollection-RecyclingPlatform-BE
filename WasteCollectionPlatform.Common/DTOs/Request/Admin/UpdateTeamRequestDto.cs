using System.ComponentModel.DataAnnotations;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public class UpdateTeamRequestDto
    {
        [Required(ErrorMessage = "Name không được để trống")]
        [StringLength(100, ErrorMessage = "Name tối đa 100 ký tự")]
        public string Name { get; set; } = null!;

        public int? AreaId { get; set; }
        public TeamType? Type { get; set; }

    }
}
