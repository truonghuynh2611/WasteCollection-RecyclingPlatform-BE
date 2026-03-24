namespace WasteCollectionPlatform.Common.DTOs.Request.Admin;

/// <summary>
/// Update admin request DTO
/// </summary>
public class UpdateAdminRequestDto
{
    /// <summary>
    /// Admin department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Admin level
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// Admin status
    /// </summary>
    public bool? Status { get; set; }
}
