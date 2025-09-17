using System;

namespace Varwin.Data
{
    public class License : IJsonSerializable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Guid { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Key { get; set; }
        public Edition? Code { get; set; }
    }
}
