using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Admin
{
    public class CancelReportRequestDto
    {
        public int ReportId { get; set; }
        public string? Reason { get; set; }
    }
}
