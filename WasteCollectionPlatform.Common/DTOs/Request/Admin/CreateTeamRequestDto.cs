using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public  class CreateTeamRequestDto
    {
        public int AreaId { get; set; }
        public string Name { get; set; }
    }
}
