using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public class UpdateAreaRequestDto
    {
        [Required(ErrorMessage = "Name không được để trống")]
        [StringLength(100, ErrorMessage = "Name tối đa 100 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "DistrictId là bắt buộc")]
        public int DistrictId { get; set; }
    }
}
