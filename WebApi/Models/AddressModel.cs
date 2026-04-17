using System;

namespace WebApi.Models
{
    public class AddressModel
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
