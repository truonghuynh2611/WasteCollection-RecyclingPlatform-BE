using System.ComponentModel.DataAnnotations;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin;

public class CreateCollectorRequestDto
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn Team")]
    public int TeamId { get; set; }

    public CollectorRole Role { get; set; } = CollectorRole.Member;
}
