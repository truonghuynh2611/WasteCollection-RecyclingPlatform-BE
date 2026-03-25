using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public class AddCollectorToTeamRequestDto
    {
        public int TeamId { get; set; }       // Team đã chọn
        public int CollectorId { get; set; }  // Collector đã tồn tại
    }
}
