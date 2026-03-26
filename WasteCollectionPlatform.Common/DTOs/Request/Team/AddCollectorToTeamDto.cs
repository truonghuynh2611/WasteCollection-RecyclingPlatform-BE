namespace WasteCollectionPlatform.Common.DTOs.Request.Team;

using WasteCollectionPlatform.Common.Enums;

public class AddCollectorToTeamDto
{
    public int TeamId { get; set; }
    public int CollectorId { get; set; }
    public CollectorRole Role { get; set; }
}
