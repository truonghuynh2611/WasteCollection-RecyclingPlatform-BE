using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public class RemoveCollectorFromTeamRequestDto
    {
        public int TeamId { get; set; }
        public int CollectorId { get; set; }
    }
}
