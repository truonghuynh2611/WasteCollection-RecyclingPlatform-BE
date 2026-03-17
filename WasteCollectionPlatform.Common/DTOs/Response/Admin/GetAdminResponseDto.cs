namespace WasteCollectionPlatform.Common.DTOs.Response.Admin;

/// <summary>
/// Get admin response DTO
/// </summary>
public class GetAdminResponseDto
{
    /// <summary>
    /// Admin ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Admin email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Admin full name
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Admin phone
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
    public bool IsSuperAdmin { get; set; }

    /// <summary>
    /// Admin status
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Created at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last login at
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
