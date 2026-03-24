namespace WasteCollectionPlatform.Common.DTOs.Request.Admin;

/// <summary>
/// Create new admin request DTO
/// </summary>
public class CreateAdminRequestDto
{
    /// <summary>
    /// Admin full name
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Admin email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Admin password
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Admin phone (optional)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Admin department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Admin level
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// Is super admin
    /// </summary>
    public bool IsSuperAdmin { get; set; } = false;
}
