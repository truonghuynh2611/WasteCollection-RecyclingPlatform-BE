using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class SubmitCompletionEvidenceDto
{
    [Required]
    public int ReportId { get; set; }

    [Required]
    public int LeaderId { get; set; }

    public List<string>? ImageUrls { get; set; } = new();

    public Microsoft.AspNetCore.Http.IFormFileCollection? ImageFiles { get; set; }

    public string? Note { get; set; }
}

public class VerifyCompletionDto
{
    [Required]
    public int ReportId { get; set; }

    [Required]
    public bool IsApproved { get; set; }

    public string? AdminNote { get; set; }
}
