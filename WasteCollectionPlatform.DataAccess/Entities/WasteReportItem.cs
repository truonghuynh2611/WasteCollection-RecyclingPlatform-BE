using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WasteCollectionPlatform.DataAccess.Entities;

[Table("WasteReportItems")]
public class WasteReportItem
{
    [Key]
    public int Id { get; set; }

    public int ReportId { get; set; }

    [Required]
    [MaxLength(50)]
    public string WasteType { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [ForeignKey("ReportId")]
    [JsonIgnore]
    public virtual WasteReport WasteReport { get; set; } = null!;
}
