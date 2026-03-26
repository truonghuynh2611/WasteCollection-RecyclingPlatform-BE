using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin;

public class CreateDistrictRequestDto
{
    [Required(ErrorMessage = "Tên quận không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên quận không được quá 100 ký tự")]
    public string DistrictName { get; set; } = string.Empty;

    public List<string>? InitialAreaNames { get; set; }
}
