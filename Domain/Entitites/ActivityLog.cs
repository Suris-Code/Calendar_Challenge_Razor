using Domain.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ActivityLog : AuditableEntity
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public string? FullUserName { get; set; }
        public string? Area { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? DisplayName { get; set; }
        public string? ActionDescriptorId { get; set; }
        public string? Name { get; set; }
        public string? Parameters { get; set; }
        public string? RemoteIPAddress { get; set; }
        public string? LocalIPAddress { get; set; }
        public string? RemotePort { get; set; }
        public string? LocalPort { get; set; }
        public int? ActivityId { get; set; }
        public string? ActivityReference { get; set; }
        public string? UserAgentFamily { get; set; }
        public string? UserAgentMajor { get; set; }
        public string? UserAgentMinor { get; set; }
        public string? UserAgentPatch { get; set; }
        public string? OSFamily { get; set; }
        public string? OSMajor { get; set; }
        public string? OSMinor { get; set; }
        public string? OSPatch { get; set; }
        public string? DeviceFamily { get; set; }
        public string? DeviceBrand { get; set; }
        public bool? DeviceIsSpider { get; set; }
        public string? DeviceModel { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
