using System;
using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.Team
{
    public class CreateCollectorDto
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đội")]
        public int TeamId { get; set; }
    }
}
