using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZLearn.Application.Common.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.LicenseKeys.DTOs
{
    public class LicenseKeyInfoDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LicenseKeyType Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LicenseKeyStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LicenseKeyLevel Level { get; set; }

        public string Key { get; set; }
        public int LifeDays { get; set; }
        public string? HardwareId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiryAt => CreatedAt.AddDays(LifeDays);
        public int RemainingDays => Math.Max(0, (int)(ExpiryAt - DateTimeOffset.UtcNow).TotalDays);
    }
}
