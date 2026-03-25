using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

[Table("SystemConfigurations")]
public partial class SystemConfiguration
{
    [Key]
    [Column("Key")]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    [Column("Value")]
    public string Value { get; set; } = null!;

    [Column("Description")]
    public string? Description { get; set; }
}
