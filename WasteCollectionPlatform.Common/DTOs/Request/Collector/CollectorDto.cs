using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Common.DTOs.Request.Collector
{
    public class CollectorDto
    {

        public int CollectorId { get; set; }
        public int UserId { get; set; }
        public int? TeamId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Leader/Member
        public bool? Status { get; set; }
    }
}
