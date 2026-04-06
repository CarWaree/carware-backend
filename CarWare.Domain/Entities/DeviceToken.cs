using CarWare.Domain.Enums;
using System;

namespace CarWare.Domain.Entities
{
    public class DeviceToken : BaseEntity
    {
        // FK + Navigation Property
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        // FCM Registeration Token
        public string Token { get; set; }  
        public DevicePlatform Platform { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedAt { get; set; }
    }

}
