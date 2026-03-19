using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Response.Admin
{
    public  class TeamResponseDto
    {
        public int TeamId { get; set; }
        public int AreaId { get; set; }
        public string Name { get; set; }
        public int CurrentTaskCount { get; set; }
    }
}
