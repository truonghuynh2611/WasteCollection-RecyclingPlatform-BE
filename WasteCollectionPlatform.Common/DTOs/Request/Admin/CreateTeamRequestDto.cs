using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public  class CreateTeamRequestDto
    {
        public int AreaId { get; set; }
        public string Name { get; set; }
        public TeamType Type { get; set; }
    }
}
